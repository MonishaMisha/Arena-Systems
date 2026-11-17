using PortfolioShowcase.Core.DI;
using PortfolioShowcase.Core.Events;
using PortfolioShowcase.Gameplay.Scoring;
using UnityEngine;

namespace PortfolioShowcase.Installers
{
    /// <summary>
    /// Single composition root: this is the only place cross-cutting services
    /// get constructed and registered. Keeping it in one spot (instead of
    /// scattering FindObjectOfType/singleton lookups through gameplay code)
    /// is what makes ScoreService and friends trivial to unit test in isolation.
    /// Drop this on a GameObject in the scene.
    ///
    /// Runs first (DefaultExecutionOrder) so services exist and subscribe to
    /// the EventBus before any consumer's Awake/OnEnable; otherwise the HUD
    /// could handle EnemyDied before ScoreService and show a stale score.
    /// Scene-scoped (no DontDestroyOnLoad): a restart reloads the scene, which
    /// tears down and rebuilds every service cleanly instead of registering twice.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class GameInstaller : MonoBehaviour
    {
        private ScoreService _scoreService;

        private void Awake()
        {
            _scoreService = new ScoreService();
            ServiceLocator.Instance.Register(_scoreService);
        }

        private void OnDestroy()
        {
            _scoreService.Dispose();
            EventBus.Clear();
            ServiceLocator.Instance.Reset();
        }
    }
}
