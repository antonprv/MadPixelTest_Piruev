// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

namespace Zenjex.Extensions.Lifecycle
{
  /// <summary>
  /// Drop-in replacement for Zenject's IInitializable.
  ///
  /// Implement this interface on any service registered in a DI container to receive
  /// a guaranteed <see cref="Initialize"/> call after the container is fully built and
  /// all dependencies are injected — but before <see cref="Core.ProjectRootInstaller.LaunchGame"/> runs.
  ///
  /// Execution order:
  ///   1. Container.Build()                 - all bindings registered
  ///   2. OnContainerReady                  - field/property injection pass
  ///   3. InstallGameInstanceRoutine()      - async setup (addressables, etc.)
  ///   4. IInitializable.Initialize()       ← you are here
  ///   5. LaunchGame()                      - user entry-point
  ///   6. OnGameLaunched                    - late injection pass
  ///
  /// Usage:
  /// <code>
  /// public class GameStateMachine : IInitializable
  /// {
  ///     private readonly StateFactory _stateFactory;
  ///
  ///     public GameStateMachine(StateFactory stateFactory) =>
  ///         _stateFactory = stateFactory;
  ///
  ///     public void Initialize() => Enter&lt;BootstrapState&gt;();
  /// }
  ///
  /// // In installer:
  /// builder.Bind&lt;GameStateMachine&gt;()
  ///        .BindInterfacesAndSelf()
  ///        .AsEagerSingleton();
  /// </code>
  ///
  /// To be discovered by <see cref="Core.ProjectRootInstaller"/>, the binding MUST
  /// expose <see cref="IInitializable"/> as a contract — use
  /// <see cref="Core.BindingBuilder{T}.BindInterfaces"/> or
  /// <see cref="Core.BindingBuilder{T}.BindInterfacesAndSelf"/>.
  /// </summary>
  public interface IInitializable
  {
    void Initialize();
  }
}
