using System;
using System.Collections.Generic;

namespace PortfolioShowcase.Core.DI
{
    /// <summary>
    /// A deliberately small service locator used as this project's composition root.
    /// It is NOT a pitch for service location over constructor injection on a larger
    /// team project (Zenject/VContainer would be the real choice there) — it's included
    /// here to demonstrate the pattern cleanly, with a single registration point
    /// (see Installers/GameInstaller) instead of scattered FindObjectOfType calls.
    /// </summary>
    public sealed class ServiceLocator
    {
        public static ServiceLocator Instance { get; } = new ServiceLocator();

        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        private ServiceLocator() { }

        public void Register<T>(T service)
        {
            var type = typeof(T);
            if (!_services.TryAdd(type, service))
            {
                throw new InvalidOperationException($"Service of type {type} is already registered.");
            }
        }

        public bool TryGet<T>(out T service)
        {
            if (_services.TryGetValue(typeof(T), out var found))
            {
                service = (T)found;
                return true;
            }

            service = default;
            return false;
        }

        public T Get<T>()
        {
            if (TryGet<T>(out var service)) return service;
            throw new InvalidOperationException($"No service of type {typeof(T)} is registered.");
        }

        /// <summary>Clears all registrations. Called between play sessions / test runs.</summary>
        public void Reset() => _services.Clear();
    }
}
