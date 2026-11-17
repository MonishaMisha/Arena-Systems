using PortfolioShowcase.Core.DI;
using PortfolioShowcase.Core.Events;
using PortfolioShowcase.Gameplay.Scoring;
using TMPro;
using UnityEngine;

namespace PortfolioShowcase.UI
{
    /// <summary>
    /// Pure listener: reacts to EventBus events and reads ScoreService via the
    /// ServiceLocator. Has no reference to the player, an enemy, or the
    /// spawner — proof the event-driven boundary actually decouples things,
    /// not just in theory.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private TMP_Text scoreLabel;
        [SerializeField] private TMP_Text waveLabel;
        [SerializeField] private TMP_Text healthLabel;
        [SerializeField] private TMP_Text bannerLabel;

        private void OnEnable()
        {
            EventBus.Subscribe<EnemyDiedEvent>(OnEnemyDied);
            EventBus.Subscribe<WaveStartedEvent>(OnWaveStarted);
            EventBus.Subscribe<PlayerDamagedEvent>(OnPlayerDamaged);
            EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
            EventBus.Subscribe<AllWavesCompletedEvent>(OnAllWavesCompleted);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<EnemyDiedEvent>(OnEnemyDied);
            EventBus.Unsubscribe<WaveStartedEvent>(OnWaveStarted);
            EventBus.Unsubscribe<PlayerDamagedEvent>(OnPlayerDamaged);
            EventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
            EventBus.Unsubscribe<AllWavesCompletedEvent>(OnAllWavesCompleted);
        }

        private void OnPlayerDied(PlayerDiedEvent e) => ShowBanner("GAME OVER");

        private void OnAllWavesCompleted(AllWavesCompletedEvent e) => ShowBanner("VICTORY");

        private void ShowBanner(string title)
        {
            if (bannerLabel != null) bannerLabel.text = $"{title}\n<size=40%>Press R to restart</size>";
        }

        private void OnEnemyDied(EnemyDiedEvent e)
        {
            if (ServiceLocator.Instance.TryGet<ScoreService>(out var score))
            {
                scoreLabel.text = $"Score: {score.Score}";
            }
        }

        private void OnWaveStarted(WaveStartedEvent e) => waveLabel.text = $"Wave {e.WaveNumber + 1}";

        private void OnPlayerDamaged(PlayerDamagedEvent e) => healthLabel.text = $"HP: {e.RemainingHealth:0}";
    }
}
