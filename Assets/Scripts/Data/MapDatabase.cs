using System;
using System.Collections.Generic;
using UnityEngine;

namespace HauntedNight.Data
{
    [Serializable]
    public class MapDefinition
    {
        public string sceneName;
        public string displayName;
        public Sprite thumbnail;
        [Tooltip("3 keys is standard; hospital uses 4")]
        public int requiredKeys = 3;
        [Range(0.8f, 1.6f)] public float enemySpeedMultiplier = 1f;
        public Sprite loadingArt;
    }

    [CreateAssetMenu(menuName = "Haunted Night/Map Database", fileName = "MapDatabase")]
    public class MapDatabase : ScriptableObject
    {
        public List<MapDefinition> maps = new();
    }
}
