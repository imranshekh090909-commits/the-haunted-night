using Unity.Netcode;
using UnityEngine;

namespace HauntedNight.Networking
{
    /// <summary>
    /// Spawns each survivor at a random safe spawn point in the current map.
    /// </summary>
    public class PlayerSpawner : NetworkBehaviour
    {
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private GameObject playerPrefab;

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            int index = 0;
            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                Transform point = spawnPoints.Length > 0
                    ? spawnPoints[index % spawnPoints.Length]
                    : transform;

                var instance = Instantiate(playerPrefab, point.position, point.rotation);
                var netObject = instance.GetComponent<NetworkObject>();
                netObject.SpawnAsPlayerObject(clientId, true);
                index++;
            }
        }
    }
}
