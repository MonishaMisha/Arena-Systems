using NUnit.Framework;
using PortfolioShowcase.Core.Combat;
using PortfolioShowcase.Core.Events;
using UnityEngine;

namespace PortfolioShowcase.Tests.EditMode
{
    public class HealthTests
    {
        private GameObject _go;
        private Health _health;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("TestSubject");
            _health = _go.AddComponent<Health>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
            EventBus.Clear();
        }

        [Test]
        public void TakeDamage_ReducesCurrentHealth()
        {
            var startHealth = _health.CurrentHealth;

            _health.TakeDamage(10f, null);

            Assert.AreEqual(startHealth - 10f, _health.CurrentHealth);
        }

        [Test]
        public void TakeDamage_CannotReduceHealthBelowZero()
        {
            _health.TakeDamage(9999f, null);

            Assert.AreEqual(0f, _health.CurrentHealth);
        }

        [Test]
        public void TakeDamage_PublishesEnemyDiedEvent_WhenHealthReachesZero()
        {
            EnemyDiedEvent? received = null;
            EventBus.Subscribe<EnemyDiedEvent>(e => received = e);

            _health.TakeDamage(9999f, null);

            Assert.IsTrue(received.HasValue);
        }

        [Test]
        public void Heal_DoesNotExceedMaxHealth()
        {
            _health.Heal(9999f);

            Assert.AreEqual(_health.MaxHealth, _health.CurrentHealth);
        }

        [Test]
        public void IsAlive_IsFalse_AfterLethalDamage()
        {
            _health.TakeDamage(9999f, null);

            Assert.IsFalse(_health.IsAlive);
        }
    }
}
