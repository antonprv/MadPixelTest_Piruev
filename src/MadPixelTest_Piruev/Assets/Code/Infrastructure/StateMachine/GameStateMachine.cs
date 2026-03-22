using BagFight.Infrastructure.StateMachine.Factory;
using BagFight.Infrastructure.StateMachine.States.Interfaces;
using Zenjex.Extensions.Lifecycle;

namespace BagFight.Infrastructure.StateMachine
{
  /// <summary>
  /// Конечный автомат игры.
  ///
  /// Жизненный цикл стейта:
  ///   1. ChangeState: вызывает Exit() на активном стейте
  ///   2. StateFactory.Create: резолвит новый стейт из Reflex-контейнера
  ///   3. Enter() / Enter(payload): запускает стейт
  ///
  /// IInitializable: Zenjex вызывает Initialize() после сборки контейнера →
  ///   сразу входим в BootstrapState.
  /// </summary>
  public class GameStateMachine : IGameStateMachine, IInitializable
  {
    public StateType CurrentState  { get; private set; }
    public StateType PreviousState { get; private set; }

    private IGameExitableState _activeState;
    private readonly StateFactory _stateFactory;

    public GameStateMachine(StateFactory stateFactory)
    {
      _stateFactory = stateFactory;
    }

    // IInitializable — вызывается Zenjex, стартует цепочку стейтов
    public void Initialize() => Enter<BootstrapState>();

    // ─── Transitions ──────────────────────────────────────────────────────────

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

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private TState ChangeState<TState>() where TState : class, IGameExitableState
    {
      _activeState?.Exit();
      PreviousState = CurrentState;

      var next = _stateFactory.Create<TState>();
      _activeState  = next;
      CurrentState  = next.Type;
      return next;
    }
  }
}
