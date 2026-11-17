using PortfolioShowcase.Core.Combat;
using PortfolioShowcase.Core.Pooling;
using UnityEngine;

namespace PortfolioShowcase.Gameplay.Weapons
{
    /// <summary>
    /// Pooled projectile. This is the high-churn object in the whole project
    /// (dozens fired per second in later waves), which is exactly the case
    /// pooling is for — contrast with EnemyController, which is intentionally
    /// NOT pooled because its lifetime spans a whole wave (see README).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour, IPoolable
    {
        private float _damage;
        private float _lifetime;
        private float _timer;
        private GameObject _owner;
        private GameObjectPool _ownerPool;
        private Rigidbody _rigidbody;

        private void Awake() => _rigidbody = GetComponent<Rigidbody>();

        public void Configure(float damage, float speed, float lifetime, GameObject owner, GameObjectPool ownerPool)
        {
            _damage = damage;
            _lifetime = lifetime;
            _owner = owner;
            _ownerPool = ownerPool;
            _rigidbody.linearVelocity = transform.forward * speed;
        }

        public void OnSpawned() => _timer = 0f;

        public void OnDespawned() => _rigidbody.linearVelocity = Vector3.zero;

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= _lifetime) ReturnToPool();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == _owner) return;

            if (other.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(_damage, _owner);
                ReturnToPool();
            }
        }

        private void ReturnToPool() => _ownerPool?.Release(gameObject);
    }
}
