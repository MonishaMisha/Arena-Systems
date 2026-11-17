using PortfolioShowcase.Core.Pooling;
using PortfolioShowcase.Data;
using UnityEngine;

namespace PortfolioShowcase.Gameplay.Weapons
{
    /// <summary>
    /// Fires projectiles pulled from a per-weapon GameObjectPool. Swapping
    /// weapons at runtime is just swapping the WeaponData asset — no code change.
    /// </summary>
    public class WeaponController : MonoBehaviour
    {
        [SerializeField] private WeaponData weaponData;
        [SerializeField] private Transform muzzle;

        private GameObjectPool _projectilePool;
        private Transform _poolRoot;
        private float _cooldownTimer;

        private void Awake()
        {
            // Pooled projectiles live under a scene-root container, NOT under the
            // shooter: parented to the player they'd be dragged and swung around
            // in flight every time the player moved or turned.
            _poolRoot = new GameObject($"{name} Projectiles").transform;
            _projectilePool = new GameObjectPool(weaponData.projectilePrefab, _poolRoot, prewarm: 10);
        }

        private void OnDestroy()
        {
            if (_poolRoot != null) Destroy(_poolRoot.gameObject);
        }

        private void Update() => _cooldownTimer -= Time.deltaTime;

        public bool TryFire()
        {
            if (_cooldownTimer > 0f) return false;

            var instance = _projectilePool.Get(muzzle.position, muzzle.rotation);
            var projectile = instance.GetComponent<Projectile>();
            projectile.Configure(
                weaponData.damage,
                weaponData.projectileSpeed,
                weaponData.projectileLifetime,
                gameObject,
                _projectilePool);

            _cooldownTimer = 1f / weaponData.fireRate;
            return true;
        }
    }
}
