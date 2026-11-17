using System;
using System.Collections.Generic;

namespace PortfolioShowcase.Core.Events
{
    /// <summary>
    /// A minimal, allocation-conscious publish/subscribe event bus.
    /// Decouples systems (UI, audio, scoring, gameplay) from the specific
    /// components that raise the events they care about, so e.g. the HUD
    /// never needs a reference to the player or an enemy to react to combat.
    ///
    /// Usage:
    ///   EventBus.Subscribe&lt;EnemyDiedEvent&gt;(OnEnemyDied);
    ///   EventBus.Publish(new EnemyDiedEvent(enemy, 10));
    ///   EventBus.Unsubscribe&lt;EnemyDiedEvent&gt;(OnEnemyDied);
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> Subscribers = new Dictionary<Type, Delegate>();

        public static void Subscribe<T>(Action<T> handler) where T : IGameEvent
        {
            var type = typeof(T);
            Subscribers[type] = Subscribers.TryGetValue(type, out var existing)
                ? Delegate.Combine(existing, handler)
                : handler;
        }

        public static void Unsubscribe<T>(Action<T> handler) where T : IGameEvent
        {
            var type = typeof(T);
            if (!Subscribers.TryGetValue(type, out var existing)) return;

            var result = Delegate.Remove(existing, handler);
            if (result == null) Subscribers.Remove(type);
            else Subscribers[type] = result;
        }

        public static void Publish<T>(T gameEvent) where T : IGameEvent
        {
            if (!Subscribers.TryGetValue(typeof(T), out var existing)) return;

            // Safe cast: only Action<T> delegates are ever stored under typeof(T).
            (existing as Action<T>)?.Invoke(gameEvent);
        }

        /// <summary>
        /// Clears every subscription. Called on scene/session teardown and between
        /// tests so listeners from a previous context never fire again.
        /// </summary>
        public static void Clear() => Subscribers.Clear();
    }
}
