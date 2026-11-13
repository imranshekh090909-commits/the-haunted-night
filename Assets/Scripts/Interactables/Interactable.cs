using Unity.Netcode;
using UnityEngine;

namespace HauntedNight.Interactables
{
    /// <summary>Base class for anything the interact button can act on.</summary>
    public abstract class Interactable : NetworkBehaviour
    {
        [SerializeField] protected string promptText = "Use";
        [SerializeField] protected float cooldown = 0.4f;

        private float _nextAllowedTime;

        public string Prompt => promptText;

        public void ServerInteract(GameObject actor)
        {
            if (Time.time < _nextAllowedTime) return;
            _nextAllowedTime = Time.time + cooldown;
            OnInteract(actor);
        }

        /// <summary>Runs on the machine that owns the interactable (server for world objects).</summary>
        protected abstract void OnInteract(GameObject actor);
    }
}
