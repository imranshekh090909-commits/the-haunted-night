using UnityEngine;

namespace HauntedNight.Interactables
{
    /// <summary>Fuse box: install a fuse piece to power the escape gate.</summary>
    public class FuseBox : Interactable
    {
        [SerializeField] private AudioClip installClip;

        private void Awake() => promptText = "Install fuse";

        protected override void OnInteract(GameObject actor)
        {
            if (!IsServer) return;
            var mission = FindObjectOfType<Objectives.MissionManager>();
            if (mission == null || mission.GatePowered) return;

            mission.ServerInstallFuse();
            if (installClip != null) Audio.AudioManager.Instance?.PlayAt(installClip, transform.position);
        }
    }
}
