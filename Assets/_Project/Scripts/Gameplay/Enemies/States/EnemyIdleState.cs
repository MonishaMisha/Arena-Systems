using PortfolioShowcase.Core.FSM;

namespace PortfolioShowcase.Gameplay.Enemies.States
{
    public class EnemyIdleState : IState
    {
        private readonly EnemyController _enemy;

        public EnemyIdleState(EnemyController enemy) => _enemy = enemy;

        public void Enter() { }

        public void Tick(float deltaTime)
        {
            if (_enemy.DistanceToTarget() <= _enemy.Data.detectionRadius)
            {
                _enemy.ChangeState<EnemyChaseState>();
            }
        }

        public void Exit() { }
    }
}
