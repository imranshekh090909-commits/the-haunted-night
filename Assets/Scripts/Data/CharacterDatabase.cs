using System;
using System.Collections.Generic;
using UnityEngine;

namespace HauntedNight.Data
{
    [Serializable]
    public class CharacterDefinition
    {
        public string id;
        public string displayName;
        public string perkDescription;
        public Sprite preview;
        public GameObject bodyPrefab;
        [Range(0.8f, 1.2f)] public float moveSpeedMultiplier = 1f;
        [Range(0.5f, 1.5f)] public float noiseMultiplier = 1f;
        [Range(0.5f, 2f)] public float flashlightBatteryMultiplier = 1f;
    }

    [CreateAssetMenu(menuName = "Haunted Night/Character Database", fileName = "CharacterDatabase")]
    public class CharacterDatabase : ScriptableObject
    {
        public List<CharacterDefinition> characters = new();
    }
}
