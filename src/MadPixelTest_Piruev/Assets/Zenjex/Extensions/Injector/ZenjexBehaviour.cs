// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using UnityEngine;

using Zenjex.Extensions.Core;
using Zenjex.Extensions.Runner;

namespace Zenjex.Extensions.Injector
{
  /// <summary>
  /// Optional base class for MonoBehaviours that use [Zenjex] injection.
  /// Provides the cleanest injection guarantee: fields are filled strictly
  /// before OnAwake() runs, with no per-frame scanning overhead.
  ///
  /// Two injection layers (cooperate, no double-injection):
  ///   1. ZenjexBehaviour (this)  - base-class Awake, strictest timing
  ///   2. ZenjexRunner            - fallback for plain MonoBehaviour classes
  ///
  /// Usage:
  ///   public class MyService : ZenjexBehaviour
  ///   {
  ///       [Zenjex] private ISomeService _service;
  ///
  ///       protected override void OnAwake() { /* _service is already injected */ }
  ///   }
  ///
  /// Initialization order:
  ///   ProjectRootInstaller.Awake()  (ExecutionOrder -280) → RootContainer built
  ///   ZenjexBehaviour.Awake()       (ExecutionOrder -100) → injection, then OnAwake()
  /// </summary>
  [DefaultExecutionOrder(-100)]
  public abstract class ZenjexBehaviour : MonoBehaviour
  {
    private void Awake()
    {
      if (RootContext.HasInstance)
      {
        ZenjexInjector.Inject(this);
        ZenjexRunner.MarkInjected(this); // tell Runner: already handled, skip scan
      }
      else
      {
        Debug.LogWarning(
          $"[Zenjex] RootContainer is not ready when {GetType().Name}.Awake() ran. " +
          "Make sure ProjectRootInstaller has a lower Script Execution Order (e.g. -280).");
      }

      OnAwake();
    }

    /// <summary>
    /// Override this instead of Awake(). Injection is already complete when this runs.
    /// </summary>
    protected virtual void OnAwake() { }
  }
}
