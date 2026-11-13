using Unity.Netcode;
using UnityEngine;

namespace HauntedNight.Player
{
    /// <summary>
    /// Battery-powered flashlight. Light is local-only (cheap), but on/off state is
    /// synced so other players see it. Battery drains; enemies are attracted by the beam.
    /// </summary>
    public class Flashlight : NetworkBehaviour
    {
        [SerializeField] private Light spotLight;
        [SerializeField] private float maxBattery = 180f;
        [SerializeField] private float drainPerSecond = 0.6f;
        [SerializeField] private float stunRange = 6f;
        [SerializeField] private float stunSeconds = 1.6f;

        public NetworkVariable<bool> IsOn = new(
            false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private float _battery;
        private float _stunCooldown;

        private void Awake() => _battery = maxBattery;

        public override void OnNetworkSpawn()
        {
            IsOn.OnValueChanged += (_, on) => Apply(on);
            Apply(IsOn.Value);
        }

        public void Toggle()
        {
            if (_battery <= 0.5f) return;
            IsOn.Value = !IsOn.Value;
            Apply(IsOn.Value);
        }

        private void Apply(bool on)
        {
            if (spotLight != null) spotLight.enabled = on;
        }

        private void Update()
        {
            if (!IsOwner) return;

            if (IsOn.Value)
            {
                _battery = Mathf.Max(0f, _battery - drainPerSecond * Time.deltaTime);
                if (_battery <= 0f) { IsOn.Value = false; Apply(false); }
            }

            _stunCooldown -= Time.deltaTime;
        }

        /// <summary>Called by the attack button when the player aims at an enemy.</summary>
        public void TryBlind()
        {
            if (!IsOwner || !IsOn.Value || _stunCooldown > 0f) return;

            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, stunRange))
            {
                var enemy = hit.collider.GetComponentInParent<AI.EnemyAI>();
                if (enemy != null)
                {
                    enemy.ServerStun(stunSeconds);
                    _stunCooldown = 3f;
                }
            }
        }

        public void AddBattery(float amount) =>
            _battery = Mathf.Min(maxBattery, _battery + amount);
    }
}
