// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System;

using Reflex.Core;

using UnityEngine;

using Zenjex.Extensions.Core;
using Zenjex.Extensions.Injector;
using Zenjex.Extensions.Lifecycle;

namespace Zenjex.Extensions.SceneContext
{
  /// <summary>
  /// Scene-scoped installer — Zenjex equivalent of Zenject's <c>SceneContext</c>.
  ///
  /// Place one per gameplay scene (on any GameObject). It creates a child container
  /// that inherits all global bindings from <see cref="ProjectRootInstaller.RootContainer"/>
  /// and adds bindings that are local to this scene only.
  ///
  /// Scene-local container is disposed automatically when the scene is unloaded.
  /// All scene-local <see cref="IInitializable"/> and <c>IDisposable</c> services are
  /// lifecycle-managed by this installer.
  ///
  /// Execution order (-200) runs after ProjectRootInstaller (-280) but before
  /// default MonoBehaviours (0), guaranteeing the scene container is ready before
  /// any regular Awake().
  ///
  /// Usage:
  /// <code>
  /// public class GameplaySceneInstaller : SceneInstaller
  /// {
  ///     [SerializeField] private LevelProgressWatcher levelProgressWatcher;
  ///
  ///     public override void InstallBindings(ContainerBuilder builder)
  ///     {
  ///         // Scene-local bindings go here
  ///         builder.BindInstance(levelProgressWatcher).AsSingle();
  ///
  ///         // Resolver that links global service to local watcher:
  ///         builder.Bind&lt;LevelProgressServiceResolver&gt;()
  ///                .BindInterfacesAndSelf()
  ///                .CopyIntoDirectSubContainers()  // scoped = fresh per scene
  ///                .NonLazy();
  ///     }
  /// }
  /// </code>
  ///
  /// Any binding registered in the global installer with
  /// <see cref="BindingBuilder{T}.CopyIntoDirectSubContainers"/> (Scoped lifetime)
  /// will be freshly instantiated inside this scene's container, letting it receive
  /// both global dependencies and the scene-local ones registered above.
  /// </summary>
  [DefaultExecutionOrder(-200)]
  public abstract class SceneInstaller : ZenjexBehaviour
  {
    /// <summary>The scoped container for this scene.</summary>
    public Container SceneContainer { get; private set; }

    /// <summary>
    /// Fires after the scene container is fully built and
    /// all scene-local <see cref="IInitializable"/> services have been initialized.
    /// Subscribe from scene-level scripts that need to react to the container being ready.
    /// </summary>
    public static event Action<Container> OnSceneContainerReady;

    protected override void OnAwake()
    {
      if (!RootContext.HasInstance)
      {
        Debug.LogError(
          $"[Zenjex] SceneInstaller on '{gameObject.name}' in scene '{gameObject.scene.name}' " +
          "ran before ProjectRootInstaller. Check Script Execution Order — " +
          "ProjectRootInstaller must be lower (e.g. -280).");
        return;
      }

      // Build a child container: inherits ALL global bindings + adds scene-local ones.
      SceneContainer = RootContext.Runtime.Scope(InstallBindings);

      // Register this scene's container for lookup by other systems.
      ZenjexSceneContext.Register(gameObject.scene, SceneContainer);

      // Call IInitializable on all scene-scoped services
      // (both fresh-scoped from global and newly registered scene-local).
      CallInitializables(SceneContainer);

      OnSceneContainerReady?.Invoke(SceneContainer);

      OnInstalled();
    }

    private void OnDestroy()
    {
      ZenjexSceneContext.Unregister(gameObject.scene);

      // Disposing the scoped container calls IDisposable on all scoped instances.
      SceneContainer?.Dispose();
      SceneContainer = null;
    }

    /// <summary>
    /// Register scene-local bindings here.
    /// The builder already has a parent set to the global RootContainer —
    /// do NOT call SetParent() manually.
    /// </summary>
    public abstract void InstallBindings(ContainerBuilder builder);

    /// <summary>
    /// Called once, right after the scene container is built and
    /// all <see cref="IInitializable"/> services have been initialized.
    /// Override to perform any scene-startup logic (optional).
    /// </summary>
    protected virtual void OnInstalled() { }

    private static void CallInitializables(Container container)
    {
      if (!container.HasBinding<IInitializable>())
        return;

      foreach (var init in container.All<IInitializable>())
      {
        try
        {
          init.Initialize();
        }
        catch (Exception ex)
        {
          Debug.LogError(
            $"[Zenjex] IInitializable.Initialize() threw on {init.GetType().Name}: {ex}");
        }
      }
    }
  }
}
