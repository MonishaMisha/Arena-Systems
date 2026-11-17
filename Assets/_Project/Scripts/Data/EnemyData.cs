using UnityEngine;

namespace PortfolioShowcase.Data
{
    /// <summary>
    /// Data-driven enemy definition. New enemy variants are authored as assets
    /// in the editor (Assets > Create > Showcase > Enemy Data) with zero new code —
    /// EnemyController and its states only ever read from this, never hardcode stats.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyData", menuName = "Showcase/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("Stats")]
        public float maxHealth = 30f;
        public float moveSpeed = 3.5f;
        public float contactDamage = 10f;

        [Header("AI")]
        public float detectionRadius = 8f;
        public float attackRange = 1.5f;
        public float attackCooldown = 1f;

        [Header("Rewards")]
        public int scoreValue = 10;
    }
}
