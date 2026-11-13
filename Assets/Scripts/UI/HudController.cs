using UnityEngine;
using UnityEngine.UI;
using HauntedNight.Player;

namespace HauntedNight.UI
{
    /// <summary>
    /// In-game HUD: stamina, keys, timer, interaction prompt, banners, damage vignette.
    /// Single instance created by the map scene, driven by GameManager network variables.
    /// </summary>
    public class HudController : MonoBehaviour
    {
        public static HudController Instance { get; private set; }

        [Header("HUD")]
        [SerializeField] private Slider staminaBar;
        [SerializeField] private Slider healthBar;
        [SerializeField] private Text keysLabel;
        [SerializeField] private Text timerLabel;
        [SerializeField] private Text promptLabel;
        [SerializeField] private Text bannerLabel;
        [SerializeField] private Image damageVignette;
        [SerializeField] private GameObject hiddenOverlay;

        private PlayerController _player;
        private PlayerHealth _health;
        private float _bannerTimer;
        private float _vignetteAlpha;
        private Camera _camera;

        private void Awake() => Instance = this;

        public void BindPlayer(PlayerController player)
        {
            _player = player;
            _health = player.GetComponent<PlayerHealth>();
            _camera = player.GetComponentInChildren<Camera>();
        }

        private void Update()
        {
            var gm = Core.GameManager.Instance;

            if (_player != null) staminaBar.value = _player.Stamina01;
            if (_health != null) healthBar.value = _health.Health.Value / 100f;

            if (gm != null)
            {
                if (keysLabel != null) keysLabel.text = $"KEYS {gm.KeysCollected.Value}/{gm.RequiredKeys}";
                if (timerLabel != null)
                {
                    float t = gm.TimeRemaining.Value;
                    timerLabel.text = $"{Mathf.FloorToInt(t / 60f):00}:{Mathf.FloorToInt(t % 60f):00}";
                }
            }

            if (_bannerTimer > 0f)
            {
                _bannerTimer -= Time.deltaTime;
                if (_bannerTimer <= 0f && bannerLabel != null) bannerLabel.text = string.Empty;
            }

            if (_vignetteAlpha > 0f && damageVignette != null)
            {
                _vignetteAlpha = Mathf.Max(0f, _vignetteAlpha - Time.deltaTime * 1.4f);
                var c = damageVignette.color;
                damageVignette.color = new Color(c.r, c.g, c.b, _vignetteAlpha);
            }
        }

        public void SetPrompt(string text)
        {
            if (promptLabel != null) promptLabel.text = text;
        }

        public void ShowBanner(string text, int kind = 0)
        {
            if (bannerLabel == null) return;
            bannerLabel.text = text;
            bannerLabel.color = kind switch
            {
                1 => new Color(0.6f, 0.95f, 0.6f),
                2 => new Color(1f, 0.35f, 0.35f),
                _ => Color.white
            };
            _bannerTimer = 3.2f;
        }

        public void FlashDamageVignette() => _vignetteAlpha = 0.65f;

        public void SetHiddenView(bool hidden, Vector3 anchor)
        {
            if (hiddenOverlay != null) hiddenOverlay.SetActive(hidden);
            if (_camera == null) return;

            if (hidden)
            {
                _camera.transform.position = anchor;
                _camera.fieldOfView = 62f;
            }
            else
            {
                _camera.fieldOfView = 72f;
            }
        }
    }
}
