using BagFight.Infrastructure.StateMachine.States.Interfaces;

namespace BagFight.Infrastructure.StateMachine
{
  public interface IGameStateMachine
  {
    StateType CurrentState  { get; }
    StateType PreviousState { get; }

    void Enter<TState>()
      where TState : class, IGameState;

    void Enter<TState, TPayload>(TPayload payload)
      where TState : class, IGamePayloadedState<TPayload>;
  }
}
