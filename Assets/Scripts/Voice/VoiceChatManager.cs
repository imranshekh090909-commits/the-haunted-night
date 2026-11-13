using System.Threading.Tasks;
using Unity.Services.Vivox;
using UnityEngine;

namespace HauntedNight.Voice
{
    /// <summary>
    /// Proximity voice chat via Vivox (Unity Gaming Services). Voices attenuate with
    /// distance, so whispering in a cupboard is actually quiet.
    /// Requires: Project Settings > Services > Vivox enabled, and the
    /// com.unity.services.vivox package in Packages/manifest.json.
    /// </summary>
    public class VoiceChatManager : MonoBehaviour
    {
        public static VoiceChatManager Instance { get; private set; }

        [SerializeField] private bool voiceEnabled = true;
        [SerializeField] private float audibleDistance = 14f;
        [SerializeField] private float conversationalDistance = 2.5f;

        private bool _loggedIn;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public async Task<bool> JoinVoiceAsync(string playerName, string channelName)
        {
            if (!voiceEnabled) return false;

            try
            {
                if (!_loggedIn)
                {
                    await VivoxService.Instance.InitializeAsync();
                    var loginOptions = new LoginOptions
                    {
                        DisplayName = playerName,
                        EnableTTS = false,
                        ParticipantUpdateFrequency = ParticipantPropertyUpdateFrequency.FivePerSecond
                    };
                    await VivoxService.Instance.LoginAsync(loginOptions);
                    _loggedIn = true;
                }

                await VivoxService.Instance.JoinGroupChannelAsync(channelName,
                    ChatCapability.AudioOnly);

                VivoxService.Instance.AudioOutputDeviceSet("Default System Device");
                ApplyProximity(true);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Voice] unavailable: {e.Message}");
                return false;
            }
        }

        public async Task LeaveVoiceAsync(string channelName)
        {
            if (!_loggedIn) return;
            try { await VivoxService.Instance.LeaveChannelAsync(channelName); }
            catch { /* ignore */ }
        }

        /// <summary>Proximity falloff - call once after joining.</summary>
        public void ApplyProximity(bool enabled)
        {
            if (!_loggedIn) return;
            VivoxService.Instance.Set3DPosition(
                Vector3.zero, audibleDistance, conversationalDistance, 1f, 1f);
        }

        public void UpdateLocalPosition(Vector3 position, Vector3 forward)
        {
            if (!_loggedIn) return;
            VivoxService.Instance.Set3DPosition(position, forward);
        }

        public void SetMuted(bool muted)
        {
            if (!_loggedIn) return;
            VivoxService.Instance.SetMuteForAll(muted ? "self" : string.Empty, muted);
        }
    }
}
