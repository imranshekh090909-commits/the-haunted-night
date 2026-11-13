using UnityEngine;
using UnityEngine.UI;

namespace HauntedNight.UI
{
    /// <summary>
    /// Character picker: skins are data-driven (ScriptableObject) so new characters
    /// cost no code - only a prefab with a swapped material set.
    /// </summary>
    public class CharacterSelectUI : MonoBehaviour
    {
        [SerializeField] private Data.CharacterDatabase database;
        [SerializeField] private Image previewImage;
        [SerializeField] private Text nameLabel;
        [SerializeField] private Text perkLabel;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button prevButton;

        private int _index;

        private void Start()
        {
            if (nextButton != null) nextButton.onClick.AddListener(() => Step(1));
            if (prevButton != null) prevButton.onClick.AddListener(() => Step(-1));
            Refresh();
        }

        private void Step(int direction)
        {
            if (database == null || database.characters.Count == 0) return;
            _index = (_index + direction + database.characters.Count) % database.characters.Count;
            Refresh();
        }

        private void Refresh()
        {
            if (database == null || database.characters.Count == 0) return;
            var character = database.characters[_index];

            if (previewImage != null) previewImage.sprite = character.preview;
            if (nameLabel != null) nameLabel.text = character.displayName;
            if (perkLabel != null) perkLabel.text = character.perkDescription;

            PlayerPrefs.SetInt("selected_character", _index);
            PlayerPrefs.Save();
        }
    }
}
