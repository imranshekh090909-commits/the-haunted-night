using Unity.Netcode;
using UnityEngine;
using HauntedNight.Player;
using HauntedNight.UI;

namespace HauntedNight.Interactables
{
    /// <summary>
    /// Cupboard / under-bed hiding. While hidden the player is invisible to enemy AI,
    /// the camera moves inside the spot, and leaving is a short, loud animation.
    /// </summary>
    public class HidingSpot : Interactable
    {
        [SerializeField] private Transform cameraAnchor;
        [SerializeField] private float exitNoiseRadius = 9f;

        public NetworkVariable<ulong> Occupant = new(
            ulong.MaxValue, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private void Awake() => promptText = "Hide";

        protected override void OnInteract(GameObject actor)
        {
            if (!IsServer) return;

            var player = actor.GetComponent<PlayerController>();
            if (player == null) return;

            if (Occupant.Value == ulong.MaxValue)
            {
                Occupant.Value = player.OwnerClientId;
                player.ServerSetHidden(true);
                EnterClientRpc(player.OwnerClientId, cameraAnchor != null ? cameraAnchor.position : transform.position);
            }
            else if (Occupant.Value == player.OwnerClientId)
            {
                Occupant.Value = ulong.MaxValue;
                player.ServerSetHidden(false);
                player.TeleportTo(transform.position + transform.forward * 0.9f, player.transform.rotation);
                AI.EnemyAI.EmitNoise(transform.position, exitNoiseRadius);
                ExitClientRpc(player.OwnerClientId);
            }
        }

        [ClientRpc]
        private void EnterClientRpc(ulong clientId, Vector3 anchor)
        {
            if (NetworkManager.Singleton.LocalClientId != clientId) return;
            HudController.Instance?.SetHiddenView(true, anchor);
        }

        [ClientRpc]
        private void ExitClientRpc(ulong clientId)
        {
            if (NetworkManager.Singleton.LocalClientId != clientId) return;
            HudController.Instance?.SetHiddenView(false, Vector3.zero);
        }
    }
}
