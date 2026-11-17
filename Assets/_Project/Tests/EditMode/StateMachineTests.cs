using NUnit.Framework;
using PortfolioShowcase.Core.FSM;

namespace PortfolioShowcase.Tests.EditMode
{
    public class StateMachineTests
    {
        private class RecordingState : IState
        {
            public int EnterCount;
            public int ExitCount;
            public int TickCount;

            public void Enter() => EnterCount++;
            public void Tick(float deltaTime) => TickCount++;
            public void Exit() => ExitCount++;
        }

        private class OtherState : IState
        {
            public void Enter() { }
            public void Tick(float deltaTime) { }
            public void Exit() { }
        }

        [Test]
        public void ChangeState_CallsEnterOnTheNewState()
        {
            var machine = new StateMachine();
            var state = new RecordingState();
            machine.Register(state);

            machine.ChangeState<RecordingState>();

            Assert.AreEqual(1, state.EnterCount);
        }

        [Test]
        public void ChangeState_CallsExitOnThePreviousState()
        {
            var machine = new StateMachine();
            var first = new RecordingState();
            machine.Register(first);
            machine.Register(new OtherState());

            machine.ChangeState<RecordingState>();
            machine.ChangeState<OtherState>();

            Assert.AreEqual(1, first.ExitCount);
        }

        [Test]
        public void ChangeState_ToSameState_IsNoOp()
        {
            var machine = new StateMachine();
            var state = new RecordingState();
            machine.Register(state);

            machine.ChangeState<RecordingState>();
            machine.ChangeState<RecordingState>();

            Assert.AreEqual(1, state.EnterCount);
        }

        [Test]
        public void Tick_ForwardsToCurrentStateOnly()
        {
            var machine = new StateMachine();
            var state = new RecordingState();
            machine.Register(state);
            machine.ChangeState<RecordingState>();

            machine.Tick(0.016f);
            machine.Tick(0.016f);

            Assert.AreEqual(2, state.TickCount);
        }

        [Test]
        public void ChangeState_ToUnregisteredState_Throws()
        {
            var machine = new StateMachine();
            Assert.Throws<System.InvalidOperationException>(() => machine.ChangeState<RecordingState>());
        }
    }
}
