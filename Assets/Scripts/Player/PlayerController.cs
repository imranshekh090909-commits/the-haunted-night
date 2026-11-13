using Unity.Netcode;
using UnityEngine;
using HauntedNight.UI;

namespace HauntedNight.Player
{
    /// <summary>
    /// Mobile-first first-person controller. Touch joystick feeds SetMoveInput,
    /// the look pad feeds SetLookInput, buttons call Jump/Interact/ToggleFlashlight.
    /// Owner-authoritative movement, server-authoritative state (hidden/alive).
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : NetworkBehaviour
    {
        [Header("Speeds")]
        [SerializeField] private float walkSpeed = 2.6f;
        [SerializeField] private float sprintSpeed = 4.4f;
        [SerializeField] private float crouchSpeed = 1.3f;
        [SerializeField] private float gravity = -18f;
        [SerializeField] private float jumpHeight = 1.0f;

        [Header("Look")]
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private float lookSensitivity = 0.12f;
        [SerializeField] private float maxPitch = 80f;

        [Header("Stamina")]
        [SerializeField] private float maxStamina = 6f;
        [SerializeField] private float staminaRegen = 1.4f;

        [Header("Refs")]
        [SerializeField] private Flashlight flashlight;
        [SerializeField] private PlayerInteractor interactor;
        [SerializeField] private AudioSource footstepSource;

        public NetworkVariable<bool> IsHidden = new(
            false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public float Stamina01 => maxStamina <= 0f ? 0f : _stamina / maxStamina;

        private CharacterController _cc;
        private Vector3 _velocity;
        private Vector2 _moveInput;
        private Vector2 _lookInput;
        private float _pitch;
        private float _stamina;
        private bool _sprinting;
        private float _stepTimer;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _stamina = maxStamina;
        }

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                // remote bodies: disable their camera + controller to save CPU on low-end phones
                if (cameraPivot != null) cameraPivot.gameObject.SetActive(false);
                _cc.enabled = false;
                enabled = false;
                return;
            }

            HudController.Instance?.BindPlayer(this);
        }

        // ---------- input API (called by TouchControls) ----------

        public void SetMoveInput(Vector2 input) => _moveInput = Vector2.ClampMagnitude(input, 1f);
        public void SetLookInput(Vector2 delta) => _lookInput = delta;
        public void SetSprint(bool value) => _sprinting = value;
        public void Jump() { if (_cc.isGrounded) _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); }
        public void Interact() => interactor?.TryInteract();
        public void ToggleFlashlight() => flashlight?.Toggle();

        // ---------- loop ----------

        private void Update()
        {
            if (!IsOwner || IsHidden.Value) return;

            ApplyLook();
            ApplyMove();
        }

        private void ApplyLook()
        {
            if (_lookInput.sqrMagnitude < 0.0001f) return;

            transform.Rotate(Vector3.up, _lookInput.x * lookSensitivity, Space.Self);
            _pitch = Mathf.Clamp(_pitch - _lookInput.y * lookSensitivity, -maxPitch, maxPitch);
            if (cameraPivot != null)
                cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
            _lookInput = Vector2.zero;
        }

        private void ApplyMove()
        {
            Vector3 wish = transform.right * _moveInput.x + transform.forward * _moveInput.y;
            if (wish.sqrMagnitude > 1f) wish.Normalize();

            bool wantsSprint = _sprinting && _moveInput.y > 0.1f && _stamina > 0.05f;
            float speed = wantsSprint ? sprintSpeed : (_moveInput.magnitude > 0.1f ? walkSpeed : 0f);

            if (wantsSprint) _stamina = Mathf.Max(0f, _stamina - Time.deltaTime);
            else _stamina = Mathf.Min(maxStamina, _stamina + staminaRegen * Time.deltaTime);

            if (_cc.isGrounded && _velocity.y < 0f) _velocity.y = -2f;
            _velocity.y += gravity * Time.deltaTime;

            _cc.Move((wish * speed + Vector3.up * _velocity.y) * Time.deltaTime);

            if (speed > 0.1f && footstepSource != null)
            {
                _stepTimer -= Time.deltaTime;
                if (_stepTimer <= 0f)
                {
                    _stepTimer = wantsSprint ? 0.32f : 0.52f;
                    footstepSource.pitch = Random.Range(0.92f, 1.08f);
                    footstepSource.Play();
                    // noise alerts nearby enemies
                    AI.EnemyAI.EmitNoise(transform.position, wantsSprint ? 12f : 6f);
                }
            }
        }

        // ---------- hiding ----------

        public void ServerSetHidden(bool hidden) => IsHidden.Value = hidden;

        public void TeleportTo(Vector3 position, Quaternion rotation)
        {
            _cc.enabled = false;
            transform.SetPositionAndRotation(position, rotation);
            _cc.enabled = true;
            _velocity = Vector3.zero;
        }
    }
}
