using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace HauntedNight.AI
{
    /// <summary>
    /// Difficulty-scaled monster spawning: more monsters with more players, hard-capped
    /// by the device tier picked in LowEndOptimizer (AI is the biggest server cost).
    /// </summary>
    public class EnemySpawner : NetworkBehaviour
    {
        [SerializeField] private GameObject[] enemyPrefabs;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private int baseCount = 2;
        [SerializeField] private int maxCount = 5;

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            {
                Debug.LogError("[EnemySpawner] No enemy prefabs assigned - map will have no monsters.");
                return;
            }

            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogError("[EnemySpawner] No spawn points assigned - spawning around the spawner instead.");
            }

            int players = Mathf.Max(1, NetworkManager.Singleton.ConnectedClientsIds.Count);
            int count = Mathf.Clamp(baseCount + (players - 1), baseCount, maxCount);
            count = Mathf.Min(count, TierCap());

            List<Transform> points = ShuffledPoints();

            for (int i = 0; i < count; i++)
            {
                GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
                if (prefab == null) continue;

                Transform point = points.Count > 0 ? points[i % points.Count] : transform;
                GameObject enemy = Instantiate(prefab, point.position, point.rotation);

                NetworkObject netObject = enemy.GetComponent<NetworkObject>();
                if (netObject == null)
                {
                    Debug.LogError($"[EnemySpawner] {prefab.name} has no NetworkObject - add it and register the prefab in NetworkManager.");
                    Destroy(enemy);
                    continue;
                }

                netObject.Spawn(true);
            }
        }

        private static int TierCap() => Perf.LowEndOptimizer.CurrentTier switch
        {
            Perf.LowEndOptimizer.Tier.Low => 2,
            Perf.LowEndOptimizer.Tier.Mid => 4,
            _ => 6
        };

        /// <summary>Shuffled copy so two monsters never start on the same point.</summary>
        private List<Transform> ShuffledPoints()
        {
            var list = new List<Transform>();
            if (spawnPoints == null) return list;

            foreach (Transform point in spawnPoints)
                if (point != null) list.Add(point);

            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
            return list;
        }
    }
}
