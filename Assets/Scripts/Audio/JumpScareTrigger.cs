using UnityEngine;
using HauntedNight.Player;

namespace HauntedNight.Audio
{
    /// <summary>One-shot scripted scare: light flicker + stinger + optional monster spawn.</summary>
    public class JumpScareTrigger : MonoBehaviour
    {
        [SerializeField] private Light[] lightsToFlicker;
        [SerializeField] private AudioClip scareClip;
        [SerializeField] private GameObject spawnOnScare;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private bool onlyOnce = true;

        private bool _fired;

        private void OnTriggerEnter(Collider other)
        {
            if (_fired && onlyOnce) return;
            if (other.GetComponentInParent<PlayerController>() == null) return;
            _fired = true;

            if (scareClip != null) AudioManager.Instance?.PlayAt(scareClip, transform.position, 1f);
            foreach (var light in lightsToFlicker)
                if (light != null) StartCoroutine(Flicker(light));

            if (spawnOnScare != null && spawnPoint != null)
            {
                var obj = Instantiate(spawnOnScare, spawnPoint.position, spawnPoint.rotation);
                obj.GetComponent<Unity.Netcode.NetworkObject>()?.Spawn(true);
            }
        }

        private System.Collections.IEnumerator Flicker(Light light)
        {
            for (int i = 0; i < 8; i++)
            {
                light.enabled = !light.enabled;
                yield return new WaitForSeconds(Random.Range(0.04f, 0.16f));
            }
            light.enabled = true;
        }
    }
}
