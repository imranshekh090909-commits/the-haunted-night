using UnityEngine;

namespace HauntedNight.AI
{
    /// <summary>
    /// The Zombie: deaf-ish but relentless, sprints in bursts, breaks line of sight
    /// by smashing through open doors. Slower turn rate so players can juke it.
    /// </summary>
    public class ZombieAI : EnemyAI
    {
        [Header("Zombie")]
        [SerializeField] private float burstInterval = 7f;
        [SerializeField] private float burstSeconds = 2.5f;
        [SerializeField] private float burstMultiplier = 1.45f;

        private float _burstTimer;
        private float _burstActive;

        protected override void Tick()
        {
            _burstTimer -= Time.deltaTime;
            _burstActive -= Time.deltaTime;

            if (_burstTimer <= 0f)
            {
                _burstTimer = burstInterval;
                _burstActive = burstSeconds;
                if (State == BrainState.Chase) Agent.speed = chaseSpeed * burstMultiplier;
            }

            if (_burstActive <= 0f && State == BrainState.Chase)
                Agent.speed = chaseSpeed;

            if (State == BrainState.Chase && Target != null)
            {
                Agent.SetDestination(Target.position);
            }
            else if (State == BrainState.Investigate)
            {
                if (ReachedDestination())
                {
                    LostTimer += Time.deltaTime;
                    if (LostTimer > searchSeconds) SetState(BrainState.Patrol);
                }
            }
            else if (State == BrainState.Patrol && ReachedDestination())
            {
                PickPatrolPoint();
            }
        }
    }
}
