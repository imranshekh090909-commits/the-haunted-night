using UnityEngine;

namespace HauntedNight.Interactables
{
    /// <summary>Mission key. Destroyed on pickup and counted by the GameManager.</summary>
    public class KeyPickup : Interactable
    {
        [SerializeField] private float bobAmplitude = 0.08f;
        [SerializeField] private float bobSpeed = 1.6f;
        [SerializeField] private AudioClip pickupClip;

        private Vector3 _startPos;

        private void Awake()
        {
            promptText = "Pick up key";
            _startPos = transform.position;
        }

        private void Update()
        {
            transform.position = _startPos + Vector3.up * Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        }

        protected override void OnInteract(GameObject actor)
        {
            if (!IsServer) return;
            Core.GameManager.Instance?.ServerRegisterKey();
            if (pickupClip != null) Audio.AudioManager.Instance?.PlayAt(pickupClip, transform.position);
            NetworkObject.Despawn(true);
        }
    }
}
