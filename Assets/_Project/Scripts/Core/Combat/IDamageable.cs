using UnityEngine;

namespace PortfolioShowcase.Core.Combat
{
    public interface IDamageable
    {
        bool IsAlive { get; }
        void TakeDamage(float amount, GameObject instigator);
    }
}
