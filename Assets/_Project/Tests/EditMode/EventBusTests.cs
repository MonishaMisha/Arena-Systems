using NUnit.Framework;
using PortfolioShowcase.Core.Events;

namespace PortfolioShowcase.Tests.EditMode
{
    public class EventBusTests
    {
        private readonly struct TestEvent : IGameEvent
        {
            public readonly int Value;
            public TestEvent(int value) => Value = value;
        }

        [TearDown]
        public void TearDown() => EventBus.Clear();

        [Test]
        public void Publish_InvokesSubscribedHandler_WithCorrectPayload()
        {
            var received = -1;
            EventBus.Subscribe<TestEvent>(e => received = e.Value);

            EventBus.Publish(new TestEvent(42));

            Assert.AreEqual(42, received);
        }

        [Test]
        public void Publish_WithNoSubscribers_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => EventBus.Publish(new TestEvent(1)));
        }

        [Test]
        public void Unsubscribe_StopsHandlerFromBeingInvoked()
        {
            var callCount = 0;
            void Handler(TestEvent e) => callCount++;

            EventBus.Subscribe<TestEvent>(Handler);
            EventBus.Unsubscribe<TestEvent>(Handler);
            EventBus.Publish(new TestEvent(1));

            Assert.AreEqual(0, callCount);
        }

        [Test]
        public void MultipleSubscribers_AllReceiveThePublishedEvent()
        {
            var firstCalled = false;
            var secondCalled = false;

            EventBus.Subscribe<TestEvent>(_ => firstCalled = true);
            EventBus.Subscribe<TestEvent>(_ => secondCalled = true);

            EventBus.Publish(new TestEvent(1));

            Assert.IsTrue(firstCalled);
            Assert.IsTrue(secondCalled);
        }
    }
}
