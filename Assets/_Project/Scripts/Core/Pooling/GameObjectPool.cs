using UnityEngine;

namespace PortfolioShowcase.Core.Pooling
{
    /// <summary>
    /// Wraps <see cref="ObjectPool{T}"/> around a specific prefab, handling the
    /// Unity-side activation/parenting so gameplay code never calls
    /// Instantiate/Destroy directly for high-churn objects like projectiles.
    /// </summary>
    public class GameObjectPool
    {
        private readonly GameObject _prefab;
        private readonly Transform _parent;
        private readonly ObjectPool<GameObject> _pool;

        public GameObjectPool(GameObject prefab, Transform parent, int prewarm = 0, int maxSize = 200)
        {
            _prefab = prefab;
            _parent = parent;
            _pool = new ObjectPool<GameObject>(Create, OnGet, OnRelease, maxSize);

            if (prewarm > 0) _pool.Prewarm(prewarm);
        }

        public GameObject Get(Vector3 position, Quaternion rotation)
        {
            var instance = _pool.Get();
            instance.transform.SetPositionAndRotation(position, rotation);
            return instance;
        }

        public void Release(GameObject instance) => _pool.Release(instance);

        private GameObject Create()
        {
            var instance = Object.Instantiate(_prefab, _parent);
            instance.SetActive(false);
            return instance;
        }

        private static void OnGet(GameObject instance)
        {
            instance.SetActive(true);
            foreach (var poolable in instance.GetComponents<IPoolable>())
            {
                poolable.OnSpawned();
            }
        }

        private static void OnRelease(GameObject instance)
        {
            foreach (var poolable in instance.GetComponents<IPoolable>())
            {
                poolable.OnDespawned();
            }
            instance.SetActive(false);
        }
    }
}
