using Reflex.Core;
using BagFight.Infrastructure.StateMachine.States.Interfaces;

namespace BagFight.Infrastructure.StateMachine.Factory
{
  /// <summary>
  /// Создаёт стейты через Reflex Container — все зависимости
  /// инжектируются автоматически из конструктора.
  /// Точная копия паттерна из оригинального проекта.
  /// </summary>
  public class StateFactory
  {
    private readonly Container _container;

    public StateFactory(Container container) => _container = container;

    public TState Create<TState>() where TState : class, IGameExitableState =>
      _container.Resolve<TState>();
  }
}
