using Unity.Netcode;
using UnityEngine;

namespace HauntedNight.AI
{
    /// <summary>
    /// Difficulty-scaled monster spawning. More monsters with more players,
    /// fewer on low-end devices (set by LowEndOptimizer).
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

            int players = NetworkManager.Singleton.ConnectedClientsIds.Count;
            int count = Mathf.Clamp(baseCount + (players - 1), baseCount, maxCount);

            for (int i = 0; i < count; i++)
            {
                var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
                var point = spawnPoints[Random.Range(0, spawnPoints.Length)];
                var enemy = Instantiate(prefab, point.position, point.rotation);
                enemy.GetComponent<NetworkObject>().Spawn(true);
            }
        }
    }
}
