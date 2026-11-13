using UnityEngine;
using HauntedNight.Interactables;
using HauntedNight.UI;

namespace HauntedNight.Player
{
    /// <summary>
    /// Raycast-based interaction. One context-sensitive button on mobile
    /// (pick up key / open door / hide / install fuse).
    /// </summary>
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private Transform rayOrigin;
        [SerializeField] private float reach = 2.4f;
        [SerializeField] private LayerMask interactMask = ~0;

        public Interactable Current { get; private set; }

        private void Update()
        {
            Current = FindTarget();
            HudController.Instance?.SetPrompt(Current != null ? Current.Prompt : string.Empty);
        }

        private Interactable FindTarget()
        {
            Transform origin = rayOrigin != null ? rayOrigin : transform;
            if (!Physics.Raycast(origin.position, origin.forward, out RaycastHit hit, reach, interactMask))
                return null;
            return hit.collider.GetComponentInParent<Interactable>();
        }

        public void TryInteract()
        {
            if (Current != null) Current.ServerInteract(gameObject);
        }
    }
}
