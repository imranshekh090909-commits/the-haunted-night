using UnityEngine;
using UnityEngine.AI;

namespace HauntedNight.AI
{
    /// <summary>
    /// The Ghost: slow, phases through walls (teleports between anchor points),
    /// flickers the victim's flashlight, and loses interest if you hide.
    /// </summary>
    public class GhostAI : EnemyAI
    {
        [Header("Ghost")]
        [SerializeField] private Transform[] phaseAnchors;
        [SerializeField] private float phaseCooldown = 14f;
        [SerializeField] private float phaseMaxDistance = 22f;

        private float _phaseTimer;

        protected override void Tick()
        {
            _phaseTimer -= Time.deltaTime;

            if (State == BrainState.Chase && Target != null)
                Agent.SetDestination(Target.position);

            if (State == BrainState.Investigate && ReachedDestination())
            {
                LostTimer += Time.deltaTime;
                if (LostTimer > searchSeconds) SetState(BrainState.Patrol);
            }

            if (State == BrainState.Patrol && ReachedDestination()) PickPatrolPoint();

            if (_phaseTimer <= 0f && State != BrainState.Chase && phaseAnchors != null && phaseAnchors.Length > 0)
            {
                _phaseTimer = phaseCooldown;
                TryPhase();
            }
        }

        private void TryPhase()
        {
            var anchor = phaseAnchors[Random.Range(0, phaseAnchors.Length)];
            if (anchor == null) return;
            if (Vector3.Distance(anchor.position, transform.position) > phaseMaxDistance) return;

            if (NavMesh.SamplePosition(anchor.position, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            {
                Agent.Warp(hit.position);
                PhaseEffectClientRpc(hit.position);
            }
        }

        [Unity.Netcode.ClientRpc]
        private void PhaseEffectClientRpc(Vector3 position)
        {
            Audio.AudioManager.Instance?.PlayAt(Audio.AudioManager.Instance.whisperClip, position);
        }
    }
}
