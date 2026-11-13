using UnityEngine;
using UnityEngine.EventSystems;
using HauntedNight.Player;

namespace HauntedNight.UI
{
    /// <summary>
    /// Mobile controls: left thumb = move stick, right thumb = look drag,
    /// buttons = interact / flashlight / sprint / blind.
    /// </summary>
    public class TouchControls : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("Move stick")]
        [SerializeField] private RectTransform stickBase;
        [SerializeField] private RectTransform stickKnob;
        [SerializeField] private float stickRadius = 110f;

        [Header("Look pad")]
        [SerializeField] private RectTransform lookPad;
        [SerializeField] private float lookScale = 0.35f;

        [Header("Buttons")]
        [SerializeField] private bool sprintHeld;

        private PlayerController _player;
        private Vector2 _move;
        private int _stickPointer = -1;
        private int _lookPointer = -1;
        private Vector2 _lastLookPos;

        private void Update()
        {
            if (_player == null)
            {
                _player = FindObjectOfType<PlayerController>();
                if (_player == null || !_player.IsOwner) { _player = null; return; }
            }
            _player.SetMoveInput(_move);
            _player.SetSprint(sprintHeld);
        }

        // ---- move stick (attach to the left half touch area) ----

        public void OnPointerDown(PointerEventData e)
        {
            if (e.position.x > Screen.width * 0.5f) { OnLookDown(e); return; }
            _stickPointer = e.pointerId;
            if (stickBase != null) stickBase.position = e.position;
        }

        public void OnDrag(PointerEventData e)
        {
            if (e.pointerId == _stickPointer)
            {
                Vector2 delta = e.position - (Vector2)stickBase.position;
                _move = Vector2.ClampMagnitude(delta / stickRadius, 1f);
                if (stickKnob != null) stickKnob.localPosition = _move * stickRadius;
            }
            else if (e.pointerId == _lookPointer)
            {
                Vector2 delta = e.position - _lastLookPos;
                _lastLookPos = e.position;
                _player?.SetLookInput(delta * lookScale);
            }
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (e.pointerId == _stickPointer)
            {
                _stickPointer = -1;
                _move = Vector2.zero;
                if (stickKnob != null) stickKnob.localPosition = Vector2.zero;
            }
            if (e.pointerId == _lookPointer) _lookPointer = -1;
        }

        private void OnLookDown(PointerEventData e)
        {
            _lookPointer = e.pointerId;
            _lastLookPos = e.position;
        }

        // ---- buttons (wire in Inspector) ----

        public void OnInteractPressed() => _player?.Interact();
        public void OnFlashlightPressed() => _player?.ToggleFlashlight();
        public void OnJumpPressed() => _player?.Jump();
        public void OnBlindPressed() => _player?.GetComponent<Flashlight>()?.TryBlind();
        public void OnSprintDown() => sprintHeld = true;
        public void OnSprintUp() => sprintHeld = false;
    }
}
