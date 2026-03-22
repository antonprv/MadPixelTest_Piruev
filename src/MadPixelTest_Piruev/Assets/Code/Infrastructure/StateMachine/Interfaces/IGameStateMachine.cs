// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Infrastructure.StateMachine.States.Types;

using Code.Infrastructure.StateMachine.States.Interfaces;

namespace Code.Infrastructure.StateMachine
{
  public interface IGameStateMachine
  {
    StateType CurrentState { get; }
    StateType PreviousState { get; }

    void Enter<TState>()
      where TState : class, IGameState;

    void Enter<TState, TPayload>(TPayload payload)
      where TState : class, IGamePayloadedState<TPayload>;
  }
}
