using Unity.Netcode;
using UnityEngine;

namespace HauntedNight.Objectives
{
    /// <summary>Escape gate: survivors standing here leave the match (needs keys + power).</summary>
    public class EscapeZone : NetworkBehaviour
    {
        [SerializeField] private MissionManager mission;
        [SerializeField] private float tickSeconds = 0.5f;

        private float _timer;

        private void Update()
        {
            if (!IsServer) return;
            _timer -= Time.deltaTime;
            if (_timer > 0f) return;
            _timer = tickSeconds;

            foreach (var client in NetworkManager.Singleton.ConnectedClientsIds)
            {
                var player = NetworkManager.Singleton.ConnectedClients[client].PlayerObject;
                if (player == null) continue;

                if (Vector3.Distance(player.transform.position, transform.position) < 2.5f)
                    mission.ServerTryEscape(client);
            }
        }
    }
}
