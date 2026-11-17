using PortfolioShowcase.Core.Events;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PortfolioShowcase.Gameplay.Flow
{
    /// <summary>
    /// Ends the run on PlayerDied / AllWavesCompleted and restarts on key press.
    /// Like the HUD, it only listens to the EventBus: it has no reference to the
    /// player, the spawner, or the HUD (which shows its own banner for the same events).
    /// </summary>
    public class GameFlowController : MonoBehaviour
    {
        [SerializeField] private KeyCode restartKey = KeyCode.R;

        private bool _isOver;

        private void OnEnable()
        {
            EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
            EventBus.Subscribe<AllWavesCompletedEvent>(OnAllWavesCompleted);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
            EventBus.Unsubscribe<AllWavesCompletedEvent>(OnAllWavesCompleted);
        }

        private void OnPlayerDied(PlayerDiedEvent e) => EndRun();

        private void OnAllWavesCompleted(AllWavesCompletedEvent e) => EndRun();

        private void EndRun()
        {
            if (_isOver) return;
            _isOver = true;
            Time.timeScale = 0f;
        }

        private void Update()
        {
            if (_isOver && Input.GetKeyDown(restartKey))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        // Never leave the editor (or the next scene) frozen.
        private void OnDestroy() => Time.timeScale = 1f;
    }
}
