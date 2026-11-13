using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace HauntedNight.Networking
{
    /// <summary>
    /// Room-code multiplayer over Unity Relay (no port forwarding, works on mobile data).
    /// Host creates a Relay allocation -> gets a 6-character join code -> clients type that code.
    /// A Lobby entry is created so players can also browse rooms later.
    /// </summary>
    public class NetworkBootstrap : MonoBehaviour
    {
        public static NetworkBootstrap Instance { get; private set; }

        public const int MaxPlayers = 4;
        public string CurrentRoomCode { get; private set; } = string.Empty;
        public string LobbyId { get; private set; } = string.Empty;
        public bool ServicesReady { get; private set; }

        private Lobby _lobby;
        private float _heartbeatTimer;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public async Task<bool> InitAsync()
        {
            if (ServicesReady) return true;
            try
            {
                if (UnityServices.State != ServicesInitializationState.Initialized)
                    await UnityServices.InitializeAsync();

                if (!AuthenticationService.Instance.IsSignedIn)
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();

                ServicesReady = true;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Net] Unity Services init failed: {e.Message}");
                return false;
            }
        }

        // ---------------- host ----------------

        public async Task<string> HostRoomAsync(string playerName, int mapIndex = 0)
        {
            if (!await InitAsync()) return null;

            try
            {
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(MaxPlayers - 1);
                CurrentRoomCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetRelayServerData(new RelayServerData(allocation, "dtls"));

                if (!NetworkManager.Singleton.StartHost())
                {
                    Debug.LogError("[Net] StartHost failed");
                    return null;
                }

                await CreateLobbyAsync(playerName, mapIndex);
                Debug.Log($"[Net] Hosting. Room code: {CurrentRoomCode}");
                return CurrentRoomCode;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Net] Host failed: {e.Message}");
                return null;
            }
        }

        private async Task CreateLobbyAsync(string playerName, int mapIndex)
        {
            var options = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = BuildPlayer(playerName),
                Data = new System.Collections.Generic.Dictionary<string, DataObject>
                {
                    { "roomCode", new DataObject(DataObject.VisibilityOptions.Member, CurrentRoomCode) },
                    { "map", new DataObject(DataObject.VisibilityOptions.Public, mapIndex.ToString()) },
                    { "state", new DataObject(DataObject.VisibilityOptions.Public, "lobby") }
                }
            };

            _lobby = await LobbyService.Instance.CreateLobbyAsync(
                $"{playerName}'s Night", MaxPlayers, options);
            LobbyId = _lobby.Id;
        }

        // ---------------- join ----------------

        public async Task<bool> JoinRoomAsync(string joinCode, string playerName)
        {
            if (!await InitAsync()) return false;

            try
            {
                joinCode = joinCode.Trim().ToUpperInvariant();
                JoinAllocation join = await RelayService.Instance.JoinAllocationAsync(joinCode);

                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetRelayServerData(new RelayServerData(join, "dtls"));

                CurrentRoomCode = joinCode;
                NetworkManager.Singleton.NetworkConfig.ConnectionData =
                    System.Text.Encoding.UTF8.GetBytes(playerName ?? "Survivor");

                return NetworkManager.Singleton.StartClient();
            }
            catch (Exception e)
            {
                Debug.LogError($"[Net] Join failed: {e.Message}");
                return false;
            }
        }

        public async Task JoinLobbyByCodeAsync(string lobbyCode, string playerName)
        {
            if (!await InitAsync()) return;
            _lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode,
                new JoinLobbyByCodeOptions { Player = BuildPlayer(playerName) });
            LobbyId = _lobby.Id;
        }

        private static Player BuildPlayer(string name) => new Player
        {
            Data = new System.Collections.Generic.Dictionary<string, PlayerDataObject>
            {
                { "name", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, name ?? "Survivor") }
            }
        };

        // ---------------- housekeeping ----------------

        private async void Update()
        {
            if (string.IsNullOrEmpty(LobbyId)) return;

            _heartbeatTimer -= Time.deltaTime;
            if (_heartbeatTimer > 0f) return;
            _heartbeatTimer = 15f;

            try { await LobbyService.Instance.SendHeartbeatPingAsync(LobbyId); }
            catch { /* lobby may be gone - ignore */ }
        }

        public async Task LeaveAsync()
        {
            try
            {
                if (!string.IsNullOrEmpty(LobbyId) && NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost)
                    await LobbyService.Instance.DeleteLobbyAsync(LobbyId);
            }
            catch { /* ignore */ }

            LobbyId = string.Empty;
            CurrentRoomCode = string.Empty;
        }
    }
}
