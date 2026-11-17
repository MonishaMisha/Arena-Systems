using NUnit.Framework;
using PortfolioShowcase.Core.Pooling;

namespace PortfolioShowcase.Tests.EditMode
{
    public class ObjectPoolTests
    {
        [Test]
        public void Get_WhenPoolIsEmpty_CreatesNewInstanceViaFactory()
        {
            var factoryCallCount = 0;
            var pool = new ObjectPool<object>(() =>
            {
                factoryCallCount++;
                return new object();
            });

            pool.Get();

            Assert.AreEqual(1, factoryCallCount);
        }

        [Test]
        public void Release_ThenGet_ReusesTheSameInstance()
        {
            var pool = new ObjectPool<object>(() => new object());
            var first = pool.Get();

            pool.Release(first);
            var second = pool.Get();

            Assert.AreSame(first, second);
        }

        [Test]
        public void CountActive_TracksItemsCurrentlyOut()
        {
            var pool = new ObjectPool<object>(() => new object());

            pool.Get();
            pool.Get();
            Assert.AreEqual(2, pool.CountActive);
        }

        [Test]
        public void Release_CalledTwiceOnSameItem_DoesNotCorruptPool()
        {
            var pool = new ObjectPool<object>(() => new object());
            var item = pool.Get();

            pool.Release(item);
            pool.Release(item); // double release should be a no-op

            Assert.AreEqual(1, pool.CountInactive);
        }

        [Test]
        public void Prewarm_PopulatesInactiveCountWithoutActivating()
        {
            var pool = new ObjectPool<object>(() => new object());

            pool.Prewarm(5);

            Assert.AreEqual(5, pool.CountInactive);
            Assert.AreEqual(0, pool.CountActive);
        }
    }
}
