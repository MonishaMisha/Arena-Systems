using UnityEngine;

namespace PortfolioShowcase.Core.Events
{
    // Domain events. Kept as readonly structs: no GC pressure from boxing,
    // and their immutability makes it obvious a subscriber can't mutate
    // shared state through the event payload.

    public readonly struct PlayerDamagedEvent : IGameEvent
    {
        public readonly float Amount;
        public readonly float RemainingHealth;

        public PlayerDamagedEvent(float amount, float remainingHealth)
        {
            Amount = amount;
            RemainingHealth = remainingHealth;
        }
    }

    public readonly struct PlayerDiedEvent : IGameEvent { }

    public readonly struct EnemyDiedEvent : IGameEvent
    {
        public readonly GameObject Enemy;
        public readonly int ScoreValue;

        public EnemyDiedEvent(GameObject enemy, int scoreValue)
        {
            Enemy = enemy;
            ScoreValue = scoreValue;
        }
    }

    public readonly struct WaveStartedEvent : IGameEvent
    {
        public readonly int WaveNumber;
        public WaveStartedEvent(int waveNumber) => WaveNumber = waveNumber;
    }

    public readonly struct WaveCompletedEvent : IGameEvent
    {
        public readonly int WaveNumber;
        public WaveCompletedEvent(int waveNumber) => WaveNumber = waveNumber;
    }

    public readonly struct AllWavesCompletedEvent : IGameEvent { }
}
