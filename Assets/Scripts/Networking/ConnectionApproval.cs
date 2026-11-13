using Unity.Netcode;
using UnityEngine;

namespace HauntedNight.Networking
{
    /// <summary>
    /// Server-side gatekeeper: caps the room at 4 players and blocks mid-match joins.
    /// </summary>
    public class ConnectionApproval : MonoBehaviour
    {
        private void Start()
        {
            if (NetworkManager.Singleton == null) return;
            NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
            NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
        }

        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request,
                                   NetworkManager.ConnectionApprovalResponse response)
        {
            bool roomFull = NetworkManager.Singleton.ConnectedClientsIds.Count >= NetworkBootstrap.MaxPlayers;
            response.Approved = !roomFull;
            response.CreatePlayerObject = true;
            response.Pending = false;
            if (roomFull) response.Reason = "Room is full (4/4).";
        }
    }
}
