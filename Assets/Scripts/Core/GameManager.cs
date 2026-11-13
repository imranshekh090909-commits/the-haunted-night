using System;
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

        private readonly HashSet<ulong> _alivePlayers = new();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                TimeRemaining.Value = missionTimeLimit;
                State.Value = MatchState.Playing;
                PlayersAlive.Value = NetworkManager.Singleton.ConnectedClientsIds.Count;
            }
        }

        private void Update()
        {
            if (!IsServer || State.Value != MatchState.Playing) return;

            TimeRemaining.Value = Mathf.Max(0f, TimeRemaining.Value - Time.deltaTime);
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
            if (!IsServer) return;
            if (!_alivePlayers.Remove(clientId)) return;

            if (_alivePlayers.Count == 0 && AllKeysFound)
            {
                State.Value = MatchState.Escaped;
                EndMatchClientRpc(true);
            }
        }

        public void ServerPlayerDied(ulong clientId, string cause)
        {
            if (!IsServer) return;
            if (!_alivePlayers.Remove(clientId)) return;
            PlayersAlive.Value = _alivePlayers.Count;
            NotifyClientRpc($"A survivor was lost: {cause}", 2);
            if (_alivePlayers.Count == 0) Wipe("No survivors left.");
        }

        private void Wipe(string reason)
        {
            if (!IsServer || State.Value != MatchState.Playing) return;
            State.Value = MatchState.Wiped;
            EndMatchClientRpc(false);
        }

        [ClientRpc]
        private void EndMatchClientRpc(bool escaped)
        {
            State.Value = escaped ? MatchState.Escaped : MatchState.Wiped;
            Debug.Log($"[Match] ended. escaped={escaped}");
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
            MapIndex.Value = Mathf.Clamp(mapIndex, 0, mapScenes.Length - 1);
            State.Value = MatchState.Loading;
            NetworkManager.Singleton.SceneManager.LoadScene(mapScenes[MapIndex.Value], LoadSceneMode.Single);
        }

        public void ServerReturnToMenu()
        {
            if (!IsServer) return;
            NetworkManager.Singleton.SceneManager.LoadScene(mainMenuScene, LoadSceneMode.Single);
        }
    }
}
