// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System;
using System.Collections;

using Reflex.Core;

using UnityEngine;

using Zenjex.Extensions.Lifecycle;

namespace Zenjex.Extensions.Core
{
  /// <summary>
  /// Abstract global composition root — Zenjex equivalent of Zenject's
  /// <c>ProjectContext</c> + <c>MonoInstaller</c>.
  ///
  /// Create one concrete subclass in your project, attach it to a persistent
  /// GameObject and implement the three abstract members.
  ///
  /// Lifecycle (guaranteed execution order):
  /// <list type="number">
  ///   <item><description>
  ///     <b>InstallBindings(builder)</b> — register all global services.
  ///     Runs synchronously inside Awake() at ExecutionOrder -280.
  ///   </description></item>
  ///   <item><description>
  ///     <b>OnContainerReady</b> — first [Zenjex] injection pass across all
  ///     loaded scenes. Because this fires inside Awake() at order -280, all
  ///     subsequent Awake() calls in the scene already see injected fields.
  ///   </description></item>
  ///   <item><description>
  ///     <b>InstallGameInstanceRoutine()</b> — async setup: load Addressables,
  ///     register runtime-only bindings (<c>RootContext.Runtime.RegisterValue</c>), etc.
  ///   </description></item>
  ///   <item><description>
  ///     <b>IInitializable.Initialize()</b> — called automatically on every
  ///     service that implements <see cref="IInitializable"/> and is registered
  ///     under that interface. Fires after step 3, before LaunchGame().
  ///   </description></item>
  ///   <item><description>
  ///     <b>LaunchGame()</b> — your game's entry point (transition to first GSM state, etc.).
  ///   </description></item>
  ///   <item><description>
  ///     <b>OnGameLaunched</b> — second [Zenjex] injection pass, covers
  ///     objects that depend on bindings added in step 3.
  ///   </description></item>
  /// </list>
  ///
  /// Usage — organise into multiple MonoInstallers attached to child GameObjects:
  /// <code>
  /// public class AppInstaller : ProjectRootInstaller
  /// {
  ///     [SerializeField] private InfrastructureInstaller _infra;
  ///     [SerializeField] private GameplayInstaller       _gameplay;
  ///
  ///     public override void InstallBindings(ContainerBuilder builder)
  ///     {
  ///         _infra.InstallBindings(builder);
  ///         _gameplay.InstallBindings(builder);
  ///     }
  ///
  ///     public override IEnumerator InstallGameInstanceRoutine()
  ///     {
  ///         yield return _infra.LoadAddressablesRoutine();
  ///     }
  ///
  ///     public override void LaunchGame() { /* GSM will start via IInitializable */ }
  /// }
  /// </code>
  /// </summary>
  [DefaultExecutionOrder(-280)]
  public abstract class ProjectRootInstaller : MonoBehaviour, IInstaller
  {
    public static Container RootContainer { get; private set; }

    /// <summary>
    /// Fires synchronously inside Awake(), immediately after RootContainer is built.
    /// All subsequent Awake() calls in the scene will already see injected [Zenjex] fields.
    /// </summary>
    public static event Action OnContainerReady;

    /// <summary>
    /// Fires after <see cref="InstallGameInstanceRoutine"/> completes and
    /// <see cref="LaunchGame"/> has been called.
    /// Use this for a second injection pass when async routines add late bindings.
    /// </summary>
    public static event Action OnGameLaunched;

    private void Awake()
    {
      if (RootContainer != null)
        return;

      var builder = new ContainerBuilder();
      InstallBindings(builder);
      RootContainer = builder.Build();

      // Pass 1: inject [Zenjex] fields in all objects already in the scene.
      OnContainerReady?.Invoke();

      StartCoroutine(LateInitRoutine());
    }

    private IEnumerator LateInitRoutine()
    {
      // Step 3: user async setup (Addressables, runtime binding, etc.)
      yield return InstallGameInstanceRoutine();

      // Step 4: call IInitializable.Initialize() on every registered service.
      // To be discovered, a service MUST expose IInitializable as a contract
      // (use BindInterfacesAndSelf() or BindInterfaces() in the installer).
      CallInitializables(RootContainer);

      // Step 5: user entry point.
      LaunchGame();

      // Pass 2: late injection for anything that depends on runtime bindings.
      OnGameLaunched?.Invoke();
    }

    #region Abstract members

    /// <summary>Register all global bindings here.</summary>
    public abstract void InstallBindings(ContainerBuilder builder);

    /// <summary>
    /// Async setup that runs between InstallBindings and LaunchGame.
    /// Yield async operations (e.g. Addressables) or return null if unused.
    /// </summary>
    public abstract IEnumerator InstallGameInstanceRoutine();

    /// <summary>
    /// Called once after all bindings are installed and IInitializables are initialized.
    /// If you use <see cref="IInitializable"/> for your GSM entry point, leave this empty.
    /// </summary>
    public abstract void LaunchGame();

    #endregion

    #region Helpers

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

    #endregion

  }
}
