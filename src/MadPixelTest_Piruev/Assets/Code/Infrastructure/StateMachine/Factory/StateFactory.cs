// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Infrastructure.StateMachine.States.Interfaces;

using Reflex.Core;

namespace Code.Infrastructure.StateMachine.Factory
{
  /// <summary>
  /// Creates states via Reflex Container — all dependencies
  /// are injected automatically from constructor.
  /// Exact copy of pattern from original project.
  /// </summary>
  public class StateFactory
  {
    private readonly Container _container;

    public StateFactory(Container container) => _container = container;

    public TState Create<TState>() where TState : class, IGameExitableState =>
      _container.Resolve<TState>();
  }
}
