namespace PortfolioShowcase.Core.Pooling
{
    /// <summary>
    /// Implemented by pooled components that need to reset their own state on
    /// spawn/despawn (timers, velocity, trails) instead of the pool needing to
    /// know each object's internals.
    /// </summary>
    public interface IPoolable
    {
        void OnSpawned();
        void OnDespawned();
    }
}
