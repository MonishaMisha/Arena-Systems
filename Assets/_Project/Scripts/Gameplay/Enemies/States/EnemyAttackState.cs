using PortfolioShowcase.Core.Combat;
using PortfolioShowcase.Core.FSM;

namespace PortfolioShowcase.Gameplay.Enemies.States
{
    public class EnemyAttackState : IState
    {
        private readonly EnemyController _enemy;
        private float _cooldownTimer;

        public EnemyAttackState(EnemyController enemy) => _enemy = enemy;

        public void Enter() => _cooldownTimer = 0f;

        public void Tick(float deltaTime)
        {
            if (_enemy.DistanceToTarget() > _enemy.Data.attackRange)
            {
                _enemy.ChangeState<EnemyChaseState>();
                return;
            }

            _cooldownTimer -= deltaTime;
            if (_cooldownTimer <= 0f)
            {
                if (_enemy.Target.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(_enemy.Data.contactDamage, _enemy.gameObject);
                }
                _cooldownTimer = _enemy.Data.attackCooldown;
            }
        }

        public void Exit() { }
    }
}
