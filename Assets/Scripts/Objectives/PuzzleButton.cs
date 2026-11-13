using UnityEngine;

namespace HauntedNight.Objectives
{
    /// <summary>
    /// Simple sequence puzzle (e.g. 4 switches in the right order) that unlocks a door
    /// or reveals a key. Server-validated so clients cannot cheat the order.
    /// </summary>
    public class PuzzleButton : Interactables.Interactable
    {
        [SerializeField] private int index;
        [SerializeField] private int[] correctOrder = { 2, 0, 3, 1 };
        [SerializeField] private Transform rewardSpawn;
        [SerializeField] private GameObject rewardPrefab;

        private static int _progress;
        private static float _lastPressTime;

        private void Awake() => promptText = "Press switch";

        protected override void OnInteract(GameObject actor)
        {
            if (!IsServer) return;

            if (Time.time - _lastPressTime > 12f) _progress = 0; // puzzle resets if idle
            _lastPressTime = Time.time;

            if (correctOrder.Length == 0) return;

            if (index == correctOrder[Mathf.Clamp(_progress, 0, correctOrder.Length - 1)])
            {
                _progress++;
                if (_progress >= correctOrder.Length)
                {
                    _progress = 0;
                    if (rewardPrefab != null && rewardSpawn != null)
                    {
                        var reward = Instantiate(rewardPrefab, rewardSpawn.position, rewardSpawn.rotation);
                        reward.GetComponent<Unity.Netcode.NetworkObject>()?.Spawn(true);
                    }
                }
            }
            else
            {
                _progress = 0;
                AI.EnemyAI.EmitNoise(transform.position, 14f); // wrong order attracts monsters
            }
        }
    }
}
