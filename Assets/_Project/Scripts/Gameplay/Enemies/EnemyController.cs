using PortfolioShowcase.Core.Combat;
using PortfolioShowcase.Core.FSM;
using PortfolioShowcase.Data;
using PortfolioShowcase.Gameplay.Enemies.States;
using UnityEngine;

namespace PortfolioShowcase.Gameplay.Enemies
{
    /// <summary>
    /// Drives a per-instance StateMachine (Idle/Chase/Attack). All tunable
    /// numbers live in EnemyData, so balancing never touches this script.
    /// Deliberately NOT pooled — see README "Trade-offs" for why pooling
    /// short-lived, low-frequency enemies would be over-engineering here.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private EnemyData data;

        public EnemyData Data => data;
        public Transform Target { get; private set; }

        private StateMachine _stateMachine;

        private void Awake()
        {
            var health = GetComponent<Health>();
            health.SetMaxHealth(data.maxHealth);
            health.Died += HandleDeath;

            _stateMachine = new StateMachine();
            _stateMachine.Register(new EnemyIdleState(this));
            _stateMachine.Register(new EnemyChaseState(this));
            _stateMachine.Register(new EnemyAttackState(this));
        }

        private void OnEnable()
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            Target = playerObj != null ? playerObj.transform : null;
            _stateMachine.ChangeState<EnemyIdleState>();
        }

        private void Update() => _stateMachine.Tick(Time.deltaTime);

        // Top-down game: distance and movement are measured on the ground plane,
        // so enemies of any size/pivot height never tilt, sink, or float toward
        // the player's capsule center.
        public float DistanceToTarget() =>
            Target == null ? Mathf.Infinity : FlatOffsetToTarget().magnitude;

        public void ChangeState<T>() where T : IState => _stateMachine.ChangeState<T>();

        public void MoveTowardsTarget(float deltaTime)
        {
            if (Target == null) return;
            var offset = FlatOffsetToTarget();
            if (offset.sqrMagnitude < 0.0001f) return;

            var direction = offset.normalized;
            transform.position += direction * (data.moveSpeed * deltaTime);
            transform.forward = direction;
        }

        private Vector3 FlatOffsetToTarget()
        {
            var offset = Target.position - transform.position;
            offset.y = 0f;
            return offset;
        }

        private void HandleDeath() => Destroy(gameObject);
    }
}
