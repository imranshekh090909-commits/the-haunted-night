using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using HauntedNight.Player;
using HauntedNight.UI;

namespace HauntedNight.AI
{
    /// <summary>
    /// Shared monster brain: patrol -> investigate noise -> chase -> attack.
    /// Server-only simulation (clients just interpolate NetworkTransform), which keeps
    /// four players + several monsters affordable on low-end phones.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class EnemyAI : NetworkBehaviour
    {
        public enum BrainState { Patrol, Investigate, Chase, Attack, Stunned }

        [Header("Senses")]
        [SerializeField] protected float sightRange = 14f;
        [SerializeField] protected float sightAngle = 110f;
        [SerializeField] protected float hearingRange = 10f;
        [SerializeField] protected LayerMask sightBlockers = ~0;

        [Header("Combat")]
        [SerializeField] protected float attackRange = 1.9f;
        [SerializeField] protected float attackDamage = 34f;
        [SerializeField] protected float attackCooldown = 1.8f;

        [Header("Patrol")]
        [SerializeField] protected Transform[] patrolPoints;
        [SerializeField] protected float patrolSpeed = 1.4f;
        [SerializeField] protected float chaseSpeed = 3.6f;
        [SerializeField] protected float searchSeconds = 6f;

        public NetworkVariable<int> StateIndex = new(
            0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        protected NavMeshAgent Agent;
        protected BrainState State = BrainState.Patrol;
        protected Transform Target;
        protected Vector3 LastKnownPosition;
        protected float LostTimer;
        protected float AttackTimer;
        protected float StunTimer;

        // global noise registry - cheap alternative to per-object physics events
        private static readonly List<(Vector3 pos, float radius)> Noises = new();

        public static void EmitNoise(Vector3 position, float radius)
        {
            if (Noises.Count > 24) Noises.RemoveAt(0);
            Noises.Add((position, radius));
        }

        public override void OnNetworkSpawn()
        {
            Agent = GetComponent<NavMeshAgent>();
            if (!IsServer)
            {
                Agent.enabled = false; // clients only render
                return;
            }
            Agent.speed = patrolSpeed;
            PickPatrolPoint();
        }

        private void Update()
        {
            if (!IsServer) return;

            if (StunTimer > 0f)
            {
                StunTimer -= Time.deltaTime;
                SetState(BrainState.Stunned);
                Agent.isStopped = true;
                return;
            }

            Agent.isStopped = false;
            AttackTimer -= Time.deltaTime;

            Sense();
            Tick();

            if (StateIndex.Value != (int)State) StateIndex.Value = (int)State;
        }

        protected abstract void Tick();

        protected void Sense()
        {
            // sight
            Transform seen = FindVisiblePlayer();
            if (seen != null)
            {
                Target = seen;
                LastKnownPosition = seen.position;
                LostTimer = 0f;
                if (Vector3.Distance(transform.position, seen.position) <= attackRange)
                {
                    SetState(BrainState.Attack);
                    TryAttack(seen);
                }
                else SetState(BrainState.Chase);
                return;
            }

            // hearing
            for (int i = Noises.Count - 1; i >= 0; i--)
            {
                var noise = Noises[i];
                if (Vector3.Distance(transform.position, noise.pos) <= Mathf.Min(noise.radius, hearingRange))
                {
                    LastKnownPosition = noise.pos;
                    LostTimer = 0f;
                    if (State != BrainState.Chase) SetState(BrainState.Investigate);
                }
                Noises.RemoveAt(i);
            }
        }

        private Transform FindVisiblePlayer()
        {
            Transform best = null;
            float bestDist = float.MaxValue;

            foreach (var client in NetworkManager.Singleton.ConnectedClientsIds)
            {
                var obj = NetworkManager.Singleton.ConnectedClients[client].PlayerObject;
                if (obj == null) continue;

                var controller = obj.GetComponent<PlayerController>();
                if (controller != null && controller.IsHidden.Value) continue; // hidden = invisible

                Vector3 toPlayer = obj.transform.position - transform.position;
                if (toPlayer.magnitude > sightRange) continue;
                if (Vector3.Angle(transform.forward, toPlayer) > sightAngle * 0.5f) continue;
                if (Physics.Linecast(transform.position + Vector3.up * 1.6f, obj.transform.position + Vector3.up * 1.2f, out RaycastHit hit, sightBlockers))
                {
                    if (!hit.transform.IsChildOf(obj.transform)) continue;
                }

                if (toPlayer.sqrMagnitude < bestDist) { bestDist = toPlayer.sqrMagnitude; best = obj.transform; }
            }
            return best;
        }

        private void TryAttack(Transform player)
        {
            if (AttackTimer > 0f) return;
            AttackTimer = attackCooldown;

            var health = player.GetComponent<PlayerHealth>();
            health?.ServerApplyDamage(attackDamage, name);
            Audio.AudioManager.Instance?.PlayAt(Audio.AudioManager.Instance.attackClip, player.position);
            JumpScareClientRpc(player.position);
        }

        [ClientRpc]
        private void JumpScareClientRpc(Vector3 position)
        {
            Audio.AudioManager.Instance?.PlayAt(Audio.AudioManager.Instance.stingerClip, position);
            HudController.Instance?.FlashDamageVignette();
        }

        protected void SetState(BrainState next)
        {
            if (State == next) return;
            State = next;

            switch (State)
            {
                case BrainState.Patrol:
                    Agent.speed = patrolSpeed;
                    PickPatrolPoint();
                    break;
                case BrainState.Investigate:
                    Agent.speed = patrolSpeed * 1.3f;
                    Agent.SetDestination(LastKnownPosition);
                    break;
                case BrainState.Chase:
                    Agent.speed = chaseSpeed;
                    break;
            }
        }

        protected void PickPatrolPoint()
        {
            if (patrolPoints == null || patrolPoints.Length == 0) return;
            var point = patrolPoints[Random.Range(0, patrolPoints.Length)];
            if (point != null) Agent.SetDestination(point.position);
        }

        protected bool ReachedDestination() =>
            !Agent.pathPending && Agent.remainingDistance <= Agent.stoppingDistance + 0.2f;

        public void ServerStun(float seconds)
        {
            if (!IsServer) return;
            StunTimer = Mathf.Max(StunTimer, seconds);
            Target = null;
        }

        public void ServerAlertTo(Vector3 position)
        {
            if (!IsServer) return;
            LastKnownPosition = position;
            if (State == BrainState.Patrol) SetState(BrainState.Investigate);
        }
    }
}
