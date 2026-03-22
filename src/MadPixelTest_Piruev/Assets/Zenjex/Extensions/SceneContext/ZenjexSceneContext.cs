// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

using Reflex.Core;

namespace Zenjex.Extensions.SceneContext
{
  /// <summary>
  /// Static registry of scene-scoped containers created by <see cref="SceneInstaller"/>.
  ///
  /// Analogous to Zenject's SceneContext — lets any global service or state machine
  /// reach into the currently loaded scene's local container without creating a
  /// direct MonoBehaviour reference.
  ///
  /// Usage:
  /// <code>
  /// // From any global service / GSM state:
  /// var sceneContainer = ZenjexSceneContext.GetActive();
  /// var watcher = sceneContainer.Resolve&lt;LevelProgressWatcher&gt;();
  /// watcher.RunLevel();
  ///
  /// // Or resolve directly:
  /// ZenjexSceneContext.Resolve&lt;LevelProgressWatcher&gt;();
  /// </code>
  /// </summary>
  public static class ZenjexSceneContext
  {
    private static readonly Dictionary<ulong, Container> _containers = new();
    private static ulong _lastRegisteredHandle = ulong.MaxValue; // sentinel "empty"

    // ── Registration (called by SceneInstaller) ───────────────────────────────

    internal static void Register(UnityEngine.SceneManagement.Scene scene, Container container)
    {
      var key = scene.handle.GetRawData();
      _containers[key] = container;
      _lastRegisteredHandle = key;
    }

    internal static void Unregister(UnityEngine.SceneManagement.Scene scene)
    {
      var key = scene.handle.GetRawData();
      _containers.Remove(key);
      if (_lastRegisteredHandle == key)
        _lastRegisteredHandle = ulong.MaxValue;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the scene-scoped container for the given scene, or null if
    /// that scene has no <see cref="SceneInstaller"/>.
    /// </summary>
    public static Container Get(UnityEngine.SceneManagement.Scene scene) =>
      _containers.TryGetValue(scene.handle.GetRawData(), out var c) ? c : null;

    /// <summary>
    /// Returns the scene-scoped container for the scene that was most recently loaded
    /// with a <see cref="SceneInstaller"/>. Useful from global services / GSM states.
    /// Returns null if no scene container is active.
    /// </summary>
    public static Container GetActive() =>
      _lastRegisteredHandle != ulong.MaxValue && _containers.TryGetValue(_lastRegisteredHandle, out var c)
        ? c
        : null;

    /// <summary>
    /// Resolves <typeparamref name="T"/> from the most recently active scene container.
    /// Throws if no scene container exists or the binding is not found.
    /// </summary>
    public static T Resolve<T>()
    {
      var container = GetActive();
      if (container == null)
        throw new System.InvalidOperationException(
          $"[Zenjex] ZenjexSceneContext.Resolve<{typeof(T).Name}>(): " +
          "no active scene container. Make sure a SceneInstaller exists in the scene.");

      return container.Resolve<T>();
    }

    /// <summary>Returns true if at least one scene container is currently registered.</summary>
    public static bool HasActiveScene => _lastRegisteredHandle != ulong.MaxValue &&
                                         _containers.ContainsKey(_lastRegisteredHandle);
  }
}
