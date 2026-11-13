using Unity.Netcode;
using UnityEngine;

namespace HauntedNight.Interactables
{
    /// <summary>Locked door: needs a key (consumes one) or opens freely if unlocked.</summary>
    public class DoorLock : Interactable
    {
        [SerializeField] private bool locked = true;
        [SerializeField] private Transform doorPivot;
        [SerializeField] private float openAngle = 95f;
        [SerializeField] private float openSpeed = 220f;
        [SerializeField] private AudioClip lockedClip;
        [SerializeField] private AudioClip openClip;

        public NetworkVariable<bool> IsOpen = new(
            false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private float _currentAngle;
        private float _targetAngle;

        private void Awake() => promptText = locked ? "Unlock door" : "Open door";

        public override void OnNetworkSpawn()
        {
            IsOpen.OnValueChanged += (_, open) => _targetAngle = open ? openAngle : 0f;
        }

        private void Update()
        {
            if (doorPivot == null) return;
            _currentAngle = Mathf.MoveTowards(_currentAngle, _targetAngle, openSpeed * Time.deltaTime);
            doorPivot.localRotation = Quaternion.Euler(0f, _currentAngle, 0f);
        }

        protected override void OnInteract(GameObject actor)
        {
            if (!IsServer) return;

            if (locked)
            {
                // keys are a team resource: 1 key opens 1 locked door
                if (Core.GameManager.Instance == null || Core.GameManager.Instance.KeysCollected.Value <= 0)
                {
                    if (lockedClip != null) Audio.AudioManager.Instance?.PlayAt(lockedClip, transform.position);
                    return;
                }
                locked = false;
                promptText = "Open door";
            }

            IsOpen.Value = !IsOpen.Value;
            if (openClip != null) Audio.AudioManager.Instance?.PlayAt(openClip, transform.position);
        }
    }
}
