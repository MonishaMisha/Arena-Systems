using System;
using System.Collections.Generic;

namespace PortfolioShowcase.Core.FSM
{
    /// <summary>
    /// A small, generic finite state machine. Engine-agnostic and fully unit
    /// testable: states only implement <see cref="IState"/>, so this class has
    /// no MonoBehaviour dependency and can be driven from any update loop.
    /// Used here for enemy AI, but equally applicable to a game-flow manager
    /// or a UI screen stack.
    /// </summary>
    public class StateMachine
    {
        public IState CurrentState { get; private set; }
        public event Action<IState, IState> StateChanged;

        private readonly Dictionary<Type, IState> _registered = new Dictionary<Type, IState>();

        public void Register<T>(T state) where T : IState => _registered[typeof(T)] = state;

        public void ChangeState<T>() where T : IState
        {
            if (!_registered.TryGetValue(typeof(T), out var next))
            {
                throw new InvalidOperationException($"State {typeof(T)} was never registered.");
            }

            if (ReferenceEquals(CurrentState, next)) return;

            var previous = CurrentState;
            previous?.Exit();
            CurrentState = next;
            CurrentState.Enter();
            StateChanged?.Invoke(previous, CurrentState);
        }

        public void Tick(float deltaTime) => CurrentState?.Tick(deltaTime);
    }
}
