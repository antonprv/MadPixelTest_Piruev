// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Infrastructure.StateMachine.States.Types;

namespace Code.Infrastructure.StateMachine.States.Interfaces
{
  /// <summary>State without payload.</summary>
  public interface IGameState : IGameExitableState
  {
    void Enter();
  }

  /// <summary>State with payload (e.g., scene name).</summary>
  public interface IGamePayloadedState<TPayload> : IGameExitableState
  {
    void Enter(TPayload payload);
  }

  /// <summary>Base interface: type + exit.</summary>
  public interface IGameExitableState
  {
    StateType Type { get; }
    void Exit();
  }
}
