using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace HauntedNight.UI
{
    /// <summary>
    /// Settings: graphics tier, render scale, sensitivity, volumes, vibration.
    /// Everything persists in PlayerPrefs so low-end phones keep their tier.
    /// </summary>
    public class SettingsUI : MonoBehaviour
    {
        [SerializeField] private AudioMixer mixer;
        [SerializeField] private Slider sensitivitySlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Dropdown qualityDropdown;
        [SerializeField] private Toggle vibrationToggle;

        private void Start()
        {
            Load();
            if (sensitivitySlider != null) sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
            if (musicSlider != null) musicSlider.onValueChanged.AddListener(v => SetVolume("MusicVolume", v));
            if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(v => SetVolume("SFXVolume", v));
            if (qualityDropdown != null) qualityDropdown.onValueChanged.AddListener(SetQuality);
            if (vibrationToggle != null) vibrationToggle.onValueChanged.AddListener(v => PlayerPrefs.SetInt("vibration", v ? 1 : 0));
        }

        private void Load()
        {
            if (sensitivitySlider != null) sensitivitySlider.value = PlayerPrefs.GetFloat("sensitivity", 1f);
            if (qualityDropdown != null) qualityDropdown.value = PlayerPrefs.GetInt("quality", QualitySettings.GetQualityLevel());
            if (vibrationToggle != null) vibrationToggle.isOn = PlayerPrefs.GetInt("vibration", 1) == 1;
        }

        private void SetSensitivity(float value)
        {
            PlayerPrefs.SetFloat("sensitivity", value);
            PlayerPrefs.Save();
        }

        private void SetVolume(string parameter, float slider01)
        {
            if (mixer == null) return;
            float db = Mathf.Log10(Mathf.Max(slider01, 0.0001f)) * 20f;
            mixer.SetFloat(parameter, db);
            PlayerPrefs.SetFloat(parameter, slider01);
        }

        private void SetQuality(int level)
        {
            QualitySettings.SetQualityLevel(level, true);
            PlayerPrefs.SetInt("quality", level);
            PlayerPrefs.Save();
        }
    }
}
