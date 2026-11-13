using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HauntedNight.Networking;

namespace HauntedNight.UI
{
    /// <summary>
    /// Main menu: Play (host / join by room code), Practice (offline), Settings, Quit.
    /// Wire these methods to your buttons in the Inspector.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject hostPanel;
        [SerializeField] private GameObject joinPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject loadingPanel;

        [Header("Fields")]
        [SerializeField] private InputField nameField;
        [SerializeField] private InputField roomCodeField;
        [SerializeField] private Text roomCodeLabel;
        [SerializeField] private Text statusLabel;
        [SerializeField] private string lobbyScene = "Map_HauntedHouse";

        public void OnPlayPressed() { mainPanel?.SetActive(false); hostPanel?.SetActive(true); }
        public void OnJoinPressed() { mainPanel?.SetActive(false); joinPanel?.SetActive(true); }
        public void OnSettingsPressed() => settingsPanel?.SetActive(true);
        public void OnBackPressed()
        {
            hostPanel?.SetActive(false);
            joinPanel?.SetActive(false);
            settingsPanel?.SetActive(false);
            mainPanel?.SetActive(true);
        }

        public async void OnCreateRoomPressed()
        {
            loadingPanel?.SetActive(true);
            string name = string.IsNullOrWhiteSpace(nameField?.text) ? "Survivor" : nameField.text.Trim();
            string code = await NetworkBootstrap.Instance.HostRoomAsync(name);

            loadingPanel?.SetActive(false);
            if (string.IsNullOrEmpty(code))
            {
                SetStatus("Could not create room. Check your internet.");
                return;
            }

            if (roomCodeLabel != null) roomCodeLabel.text = $"ROOM CODE: {code}";
            SceneManager.LoadScene(lobbyScene);
        }

        public async void OnJoinRoomPressed()
        {
            loadingPanel?.SetActive(true);
            string name = string.IsNullOrWhiteSpace(nameField?.text) ? "Survivor" : nameField.text.Trim();
            string code = roomCodeField != null ? roomCodeField.text : string.Empty;

            bool ok = await NetworkBootstrap.Instance.JoinRoomAsync(code, name);
            loadingPanel?.SetActive(false);

            if (!ok) { SetStatus("Room not found. Check the 6-letter code."); return; }
            SceneManager.LoadScene(lobbyScene);
        }

        public void OnPracticePressed()
        {
            // offline: start the map scene directly, no NetworkManager started
            SceneManager.LoadScene(lobbyScene);
        }

        public void OnQuitPressed()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        private void SetStatus(string text)
        {
            if (statusLabel != null) statusLabel.text = text;
            Debug.Log($"[Menu] {text}");
        }
    }
}
