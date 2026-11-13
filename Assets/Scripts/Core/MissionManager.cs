using Unity.Netcode;
using UnityEngine;
using HauntedNight.Core;

namespace HauntedNight.Objectives
{
    /// <summary>
    /// Per-map objective bookkeeping: keys, fuse puzzle, and the escape gate.
    /// Lives on a "Mission" object inside each map scene.
    /// </summary>
    public class MissionManager : NetworkBehaviour
    {
        [SerializeField] private int fusePiecesNeeded = 2;
        [SerializeField] private EscapeZone escapeZone;

        public NetworkVariable<int> FusesInstalled = new(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public bool GatePowered => FusesInstalled.Value >= fusePiecesNeeded;

        public void ServerInstallFuse()
        {
            if (!IsServer) return;
            FusesInstalled.Value = Mathf.Min(FusesInstalled.Value + 1, fusePiecesNeeded);
        }

        public bool ServerCanEscape() => GameManager.Instance != null &&
                                         GameManager.Instance.AllKeysFound &&
                                         GatePowered;

        public void ServerTryEscape(ulong clientId)
        {
            if (!IsServer) return;
            if (!ServerCanEscape()) return;
            GameManager.Instance.ServerPlayerEscaped(clientId);
        }
    }
}
