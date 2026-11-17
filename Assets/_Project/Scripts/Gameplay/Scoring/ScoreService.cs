using PortfolioShowcase.Core.Events;

namespace PortfolioShowcase.Gameplay.Scoring
{
    /// <summary>
    /// Plain C# class (no MonoBehaviour) that tracks score by listening to
    /// domain events on the EventBus. Because it has zero Unity dependency it
    /// can be constructed and unit tested without a scene, a GameObject, or a
    /// running player loop — see how EventBusTests exercises the same pattern.
    /// Registered with the ServiceLocator by GameInstaller at boot.
    /// </summary>
    public class ScoreService
    {
        public int Score { get; private set; }

        public ScoreService()
        {
            EventBus.Subscribe<EnemyDiedEvent>(OnEnemyDied);
        }

        private void OnEnemyDied(EnemyDiedEvent e) => Score += e.ScoreValue;

        public void Dispose() => EventBus.Unsubscribe<EnemyDiedEvent>(OnEnemyDied);
    }
}
