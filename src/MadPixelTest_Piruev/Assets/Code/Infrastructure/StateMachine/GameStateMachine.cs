// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Infrastructure.StateMachine.Factory;
using Code.Infrastructure.StateMachine.States.Interfaces;
using Code.Infrastructure.StateMachine.States.Types;

namespace Code.Infrastructure.StateMachine
{
  /// <summary>
  /// Game state machine.
  ///
  /// State lifecycle:
  ///   1. ChangeState: calls Exit() on active state
  ///   2. StateFactory.Create: resolves new state from Reflex container
  ///   3. Enter() / Enter(payload): starts the state
  ///
  /// IInitializable: Zenjex calls Initialize() after container assembly —
  ///   immediately enter BootstrapState.
  /// </summary>
  public class GameStateMachine : IGameStateMachine
  {
    public StateType CurrentState { get; private set; }
    public StateType PreviousState { get; private set; }

    private IGameExitableState _activeState;
    private readonly StateFactory _stateFactory;

    public GameStateMachine(StateFactory stateFactory) => _stateFactory = stateFactory;

    #region Transitions

    public void Enter<TState>() where TState : class, IGameState
    {
      var state = ChangeState<TState>();
      state.Enter();
    }

    public void Enter<TState, TPayload>(TPayload payload)
      where TState : class, IGamePayloadedState<TPayload>
    {
      var state = ChangeState<TState>();
      state.Enter(payload);
    }

    #endregion

    #region Helpers

    private TState ChangeState<TState>() where TState : class, IGameExitableState
    {
      _activeState?.Exit();
      PreviousState = CurrentState;

      var next = _stateFactory.Create<TState>();
      _activeState = next;
      CurrentState = next.Type;
      return next;
    }

    #endregion
  }
}
