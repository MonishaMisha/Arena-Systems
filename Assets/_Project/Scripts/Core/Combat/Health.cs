using System;
using PortfolioShowcase.Core.Events;
using UnityEngine;

namespace PortfolioShowcase.Core.Combat
{
    /// <summary>
    /// Generic health component shared by the player and every enemy type.
    /// Publishes domain events instead of calling into other systems directly
    /// (no reference to a HUD, a game-over screen, or a score counter here),
    /// and exposes a local <see cref="Died"/> event for the owning entity to
    /// decide its own destruction policy — see EnemyController.HandleDeath.
    /// Keeping "what happens on death" out of Health is what let this class
    /// stay trivially unit-testable (see Tests/EditMode/HealthTests.cs).
    /// </summary>
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private bool isPlayer;
        [SerializeField] private int scoreValueOnDeath = 10;

        public float MaxHealth => maxHealth;
        public float CurrentHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0f;

        public event Action<float, float> HealthChanged; // (current, max)
        public event Action Died;

        private void Awake() => CurrentHealth = maxHealth;

        public void SetMaxHealth(float value, bool refill = true)
        {
            maxHealth = value;
            if (refill) CurrentHealth = maxHealth;
        }

        public void TakeDamage(float amount, GameObject instigator)
        {
            if (!IsAlive || amount <= 0f) return;

            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            HealthChanged?.Invoke(CurrentHealth, maxHealth);

            if (isPlayer)
            {
                EventBus.Publish(new PlayerDamagedEvent(amount, CurrentHealth));
            }

            if (!IsAlive) Die();
        }

        public void Heal(float amount)
        {
            if (!IsAlive || amount <= 0f) return;
            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        private void Die()
        {
            if (isPlayer) EventBus.Publish(new PlayerDiedEvent());
            else EventBus.Publish(new EnemyDiedEvent(gameObject, scoreValueOnDeath));

            Died?.Invoke();
        }
    }
}
