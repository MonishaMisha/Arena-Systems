using PortfolioShowcase.Core.FSM;

namespace PortfolioShowcase.Gameplay.Enemies.States
{
    public class EnemyChaseState : IState
    {
        private readonly EnemyController _enemy;

        public EnemyChaseState(EnemyController enemy) => _enemy = enemy;

        public void Enter() { }

        public void Tick(float deltaTime)
        {
            var distance = _enemy.DistanceToTarget();

            if (distance <= _enemy.Data.attackRange)
            {
                _enemy.ChangeState<EnemyAttackState>();
                return;
            }

            if (distance > _enemy.Data.detectionRadius * 1.5f)
            {
                _enemy.ChangeState<EnemyIdleState>();
                return;
            }

            _enemy.MoveTowardsTarget(deltaTime);
        }

        public void Exit() { }
    }
}
