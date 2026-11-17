using System;
using System.Collections.Generic;

namespace PortfolioShowcase.Core.Pooling
{
    /// <summary>
    /// A generic, engine-agnostic object pool. Deliberately free of UnityEngine
    /// references so its core logic (reuse, growth, double-release protection)
    /// can be unit tested in EditMode without instantiating a single GameObject.
    /// <see cref="GameObjectPool"/> wraps this for prefab-based pooling.
    /// </summary>
    public class ObjectPool<T>
    {
        private readonly Stack<T> _inactive = new Stack<T>();
        private readonly HashSet<T> _active = new HashSet<T>();
        private readonly Func<T> _factory;
        private readonly Action<T> _onGet;
        private readonly Action<T> _onRelease;
        private readonly int _maxSize;

        public int CountActive => _active.Count;
        public int CountInactive => _inactive.Count;

        public ObjectPool(Func<T> factory, Action<T> onGet = null, Action<T> onRelease = null, int maxSize = 100)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _onGet = onGet;
            _onRelease = onRelease;
            _maxSize = maxSize;
        }

        public T Get()
        {
            var item = _inactive.Count > 0 ? _inactive.Pop() : _factory();
            _active.Add(item);
            _onGet?.Invoke(item);
            return item;
        }

        public void Release(T item)
        {
            // Guards against a double-release corrupting the pool (e.g. a projectile
            // that hits something the same frame its lifetime timer expires).
            if (!_active.Remove(item)) return;

            _onRelease?.Invoke(item);

            if (_inactive.Count < _maxSize)
            {
                _inactive.Push(item);
            }
        }

        public void Prewarm(int count)
        {
            for (var i = 0; i < count; i++)
            {
                _inactive.Push(_factory());
            }
        }
    }
}
