namespace BagFight.Infrastructure.StateMachine.States.Interfaces
{
  /// <summary>Стейт без payload.</summary>
  public interface IGameState : IGameExitableState
  {
    void Enter();
  }

  /// <summary>Стейт с payload (например, имя сцены).</summary>
  public interface IGamePayloadedState<TPayload> : IGameExitableState
  {
    void Enter(TPayload payload);
  }

  /// <summary>Базовый интерфейс: тип + выход.</summary>
  public interface IGameExitableState
  {
    StateType Type { get; }
    void Exit();
  }
}
