using Unity.Netcode;
using UnityEngine;
using HauntedNight.UI;

namespace HauntedNight.Player
{
    /// <summary>
    /// Server-authoritative health with downed state and revive by teammates.
    /// </summary>
    public class PlayerHealth : NetworkBehaviour
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float reviveSeconds = 5f;

        public NetworkVariable<float> Health = new(
            100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public NetworkVariable<bool> IsDown = new(
            false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public override void OnNetworkSpawn()
        {
            if (IsServer) Health.Value = maxHealth;
            IsDown.OnValueChanged += (_, down) => OnDownChanged(down);
        }

        public void ServerApplyDamage(float amount, string cause)
        {
            if (!IsServer || IsDown.Value) return;

            Health.Value = Mathf.Max(0f, Health.Value - amount);
            if (Health.Value <= 0f)
            {
                IsDown.Value = true;
                StartCoroutine(BleedOut());
            }
        }

        private System.Collections.IEnumerator BleedOut()
        {
            yield return new WaitForSeconds(reviveSeconds * 6f);
            if (IsDown.Value && IsServer)
            {
                Core.GameManager.Instance?.ServerPlayerDied(OwnerClientId, "killed by a monster");
                NetworkObject.Despawn(true);
            }
        }

        public void ServerRevive(float amount)
        {
            if (!IsServer) return;
            Health.Value = Mathf.Min(maxHealth, Health.Value + amount);
            if (IsDown.Value && Health.Value > maxHealth * 0.35f) IsDown.Value = false;
        }

        private void OnDownChanged(bool down)
        {
            var controller = GetComponent<PlayerController>();
            if (controller != null && controller.IsOwner)
                HudController.Instance?.ShowBanner("You are down! Crawl to a teammate.", 2);
        }
    }
}
