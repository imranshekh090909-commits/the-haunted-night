using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using HauntedNight.UI;

namespace HauntedNight.Core
{
    /// <summary>
    /// Central authority for match flow: lobby -> mission -> escape / wipe -> results.
    /// Server owns the state machine, clients only render it.
    /// </summary>
    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance { get; private set; }

        public enum MatchState { Lobby, Loading, Playing, Escaped, Wiped }
        public enum MapId { HauntedHouse = 0, AbandonedHospital = 1 }

        [Header("Scenes")]
        [SerializeField] private string mainMenuScene = "MainMenu";
        [SerializeField] private string[] mapScenes = { "Map_HauntedHouse", "Map_AbandonedHospital" };

        [Header("Rules")]
        [SerializeField] private float missionTimeLimit = 900f; // 15 min
        [SerializeField] private int requiredKeys = 3;

        public NetworkVariable<MatchState> State = new(
            MatchState.Lobby,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public NetworkVariable<float> TimeRemaining = new(
            900f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public NetworkVariable<int> KeysCollected = new(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public NetworkVariable<int> PlayersAlive = new(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public NetworkVariable<int> MapIndex = new(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public int RequiredKeys => requiredKeys;

        /// <summary>Survivors still in the match. Server-side truth; PlayersAlive mirrors it.</summary>
        private readonly HashSet<ulong> _alivePlayers = new();
        private float _timerAccumulator;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            // seed the alive set - without this the set stays empty and nobody can ever
            // escape or die, which silently disables the win/lose conditions
            _alivePlayers.Clear();
            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
                _alivePlayers.Add(clientId);

            _timerAccumulator = 0f;
            TimeRemaining.Value = missionTimeLimit;
            KeysCollected.Value = 0;
            PlayersAlive.Value = _alivePlayers.Count;
            State.Value = MatchState.Playing;
        }

        /// <summary>Called by PlayerSpawner for every player object it creates.</summary>
        public void ServerRegisterPlayer(ulong clientId)
        {
            if (!IsServer) return;
            if (!_alivePlayers.Add(clientId)) return;
            PlayersAlive.Value = _alivePlayers.Count;
        }

        private void Update()
        {
            if (!IsServer || State.Value != MatchState.Playing) return;

            // write the timer about once a second instead of every frame - a per-frame
            // NetworkVariable write is 60 redundant packets/second per client
            _timerAccumulator += Time.deltaTime;
            if (_timerAccumulator < 1f) return;

            _timerAccumulator = 0f;
            TimeRemaining.Value = Mathf.Max(0f, TimeRemaining.Value - 1f);
            if (TimeRemaining.Value <= 0f) Wipe("Time ran out - the night took you.");
        }

        // ---------- keys & objectives ----------

        public void ServerRegisterKey()
        {
            if (!IsServer || State.Value != MatchState.Playing) return;
            KeysCollected.Value = Mathf.Min(KeysCollected.Value + 1, requiredKeys);
            NotifyClientRpc($"Key found ({KeysCollected.Value}/{requiredKeys})", 1);
        }

        public bool AllKeysFound => KeysCollected.Value >= requiredKeys;

        // ---------- win / lose ----------

        public void ServerPlayerEscaped(ulong clientId)
        {
            if (!IsServer || State.Value != MatchState.Playing) return;
            if (!_alivePlayers.Remove(clientId)) return;

            PlayersAlive.Value = _alivePlayers.Count;
            NotifyClientRpc("A survivor reached the exit.", 1);

            if (_alivePlayers.Count == 0 && AllKeysFound)
            {
                State.Value = MatchState.Escaped;
                EndMatchClientRpc(true, "Everyone got out. The night lost.");
            }
        }

        public void ServerPlayerDied(ulong clientId, string cause)
        {
            if (!IsServer || State.Value != MatchState.Playing) return;
            if (!_alivePlayers.Remove(clientId)) return;

            PlayersAlive.Value = _alivePlayers.Count;
            NotifyClientRpc($"A survivor was lost: {cause}", 2);
            if (_alivePlayers.Count == 0) Wipe("No survivors left.");
        }

        private void Wipe(string reason)
        {
            if (!IsServer || State.Value != MatchState.Playing) return;
            State.Value = MatchState.Wiped;
            EndMatchClientRpc(false, reason);
        }

        [ClientRpc]
        private void EndMatchClientRpc(bool escaped, string reason)
        {
            // clients must NOT write State: it is a server-write NetworkVariable, so a
            // client assignment throws and the value is replicated anyway
            Debug.Log($"[Match] ended. escaped={escaped}. {reason}");
            HudController.Instance?.ShowBanner(reason, escaped ? 1 : 2);
        }

        [ClientRpc]
        private void NotifyClientRpc(string message, int kind)
        {
            // kind: 0 = info, 1 = objective, 2 = danger
            HudController.Instance?.ShowBanner(message, kind);
        }

        // ---------- scene flow (server only) ----------

        public void ServerLoadMap(int mapIndex)
        {
            if (!IsServer) return;
            if (mapScenes == null || mapScenes.Length == 0)
            {
                Debug.LogError("[GameManager] No map scenes configured.");
                return;
            }

            MapIndex.Value = Mathf.Clamp(mapIndex, 0, mapScenes.Length - 1);
            State.Value = MatchState.Loading;
            NetworkManager.Singleton.SceneManager.LoadScene(mapScenes[MapIndex.Value], LoadSceneMode.Single);
        }

        public void ServerReturnToMenu()
        {
            if (!IsServer) return;
            State.Value = MatchState.Lobby;
            NetworkManager.Singleton.SceneManager.LoadScene(mainMenuScene, LoadSceneMode.Single);
        }
    }
}
