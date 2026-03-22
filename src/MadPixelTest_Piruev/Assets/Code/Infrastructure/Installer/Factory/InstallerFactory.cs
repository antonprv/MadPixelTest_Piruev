// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System;
using System.Collections;

using Code.Infrastructure.Installer.Addresses;
using Code.Infrastructure.Loading;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using UObject = UnityEngine.Object;

namespace Code.Infrastructure.Installer.Factory
{
  /// <summary>
  /// Responsible for async Addressable loading and instantiation of
  /// infrastructure prefabs that must exist before the game loop starts.
  ///
  /// Two separate routines keep concerns isolated:
  ///   1. <see cref="CreateLoadingScreenRoutine"/> — instantiates the curtain and
  ///      returns <see cref="ILoadScreen"/>. Must be called first so the caller can
  ///      register ILoadScreen in the DI container before GameInstance is created.
  ///   2. <see cref="CreateGameInstanceRoutine"/> — instantiates GameInstance.
  ///      At this point ILoadScreen is already in the container, so
  ///      ZenjexBehaviour.Awake() resolves the [Zenjex] field automatically.
  /// </summary>
  public static class InstallerFactory
  {
    private static AsyncOperationHandle<GameObject> _loadingScreenHandle;
    private static AsyncOperationHandle<GameObject> _gameInstanceHandle;

    #region Loading Screen

    /// <summary>
    /// Loads and instantiates the LoadingScreen prefab.
    /// Call this before <see cref="CreateGameInstanceRoutine"/> and register
    /// the result as ILoadScreen in the DI container immediately after.
    /// </summary>
    public static IEnumerator CreateLoadingScreenRoutine(Action<ILoadScreen> onComplete)
    {
      _loadingScreenHandle =
          Addressables.LoadAssetAsync<GameObject>(InstallerAddresses.LoadingScreenAddress);

      yield return _loadingScreenHandle;

      if (_loadingScreenHandle.Status != AsyncOperationStatus.Succeeded)
      {
        Debug.LogError($"{nameof(InstallerFactory)}: Failed to load LoadingScreen prefab.");
        onComplete?.Invoke(null);
        yield break;
      }

      GameObject go = UObject.Instantiate(_loadingScreenHandle.Result);
      UObject.DontDestroyOnLoad(go);

      onComplete?.Invoke(go.GetComponent<ILoadScreen>());
    }

    #endregion

    #region Game Instance

    /// <summary>
    /// Loads and instantiates the GameInstance prefab.
    ///
    /// ILoadScreen MUST be registered in RootContainer before calling this.
    ///
    /// The prefab is temporarily deactivated so Awake() does NOT fire during
    /// Instantiate(). <paramref name="onBeforeActivate"/> is called first, giving
    /// the caller a window to register runtime bindings (ICoroutineRunner,
    /// FramerateManager, etc.). Only then is the instance activated, so
    /// ZenjexBehaviour.Awake() runs with a fully populated container.
    /// </summary>
    public static IEnumerator CreateGameInstanceRoutine(
        Action<GameInstance> onBeforeActivate,
        Action<GameInstance> onComplete = null)
    {
      _gameInstanceHandle =
          Addressables.LoadAssetAsync<GameObject>(InstallerAddresses.GameInstanceAddress);

      yield return _gameInstanceHandle;

      if (_gameInstanceHandle.Status != AsyncOperationStatus.Succeeded)
      {
        Debug.LogError($"{nameof(InstallerFactory)}: Failed to load GameInstance prefab.");
        onBeforeActivate?.Invoke(null);
        onComplete?.Invoke(null);
        yield break;
      }

      // Temporarily deactivate the prefab so Instantiate() won't fire Awake()
      // before we get a chance to register the remaining runtime bindings.
      bool wasActive = _gameInstanceHandle.Result.activeSelf;
      _gameInstanceHandle.Result.SetActive(false);

      GameObject go = UObject.Instantiate(_gameInstanceHandle.Result);
      UObject.DontDestroyOnLoad(go);

      // Restore the prefab to its original state (doesn't affect our instance).
      _gameInstanceHandle.Result.SetActive(wasActive);

      var instance = go.GetComponent<GameInstance>();

      // Register runtime bindings BEFORE the instance goes live.
      // ZenjexBehaviour.Awake() will fire below and will find everything it needs.
      onBeforeActivate?.Invoke(instance);

      // Activation: ZenjexBehaviour.Awake() → ZenjexInjector.Inject() fires here
      // with ICoroutineRunner, FramerateManager etc. already in the container.
      go.SetActive(true);

      onComplete?.Invoke(instance);
    }

    #endregion

    #region Cleanup

    public static void Release()
    {
      if (_loadingScreenHandle.IsValid())
        Addressables.Release(_loadingScreenHandle);

      if (_gameInstanceHandle.IsValid())
        Addressables.Release(_gameInstanceHandle);
    }

    #endregion
  }
}
