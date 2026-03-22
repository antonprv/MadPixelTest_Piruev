// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System;
using System.Threading;

using Cysharp.Threading.Tasks;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Code.Infrastructure.SceneLoader
{
  /// <summary>
  /// Loads scenes via Addressables in LoadSceneMode.Single.
  ///
  /// LoadSceneMode.Single tells Unity to unload the current scene automatically
  /// as part of loading the new one — the same way SceneManager.LoadScene works.
  /// We never call Addressables.Release on a SceneInstance handle because that
  /// tries to unload the scene explicitly, which Unity forbids for the last
  /// loaded scene ("Unloading the last loaded scene is not supported").
  ///
  /// The previous handle is stored only so we can check IsDone / Status;
  /// we intentionally do NOT release it.
  ///
  /// Progress overload: polls handle.PercentComplete each frame so
  /// LoadLevelState can push granular updates to the loading curtain.
  /// </summary>
  public class AddressableSceneLoader : ISceneLoader
  {
    private AsyncOperationHandle<SceneInstance> _currentHandle;
    private bool _hasHandle;

    #region Without progress

    public UniTask LoadAsync(string address, CancellationToken ct = default) =>
      LoadAsync(address, ct, null);

    #endregion

    #region With progress

    public async UniTask LoadAsync(
      string            address,
      CancellationToken ct,
      IProgress<float>  progress)
    {
      if (SceneManager.GetActiveScene().name == address)
      {
        Debug.Log($"[SceneLoader] '{address}' is already active. Skipping.");
        progress?.Report(1f);
        return;
      }

      // LoadSceneMode.Single: Unity unloads the current scene automatically.
      // Do NOT call Addressables.Release on the previous handle — that would
      // trigger an explicit unload which throws if the scene is still active.
      var handle = Addressables.LoadSceneAsync(address, LoadSceneMode.Single);

      _currentHandle = handle;
      _hasHandle     = true;

      while (!handle.IsDone)
      {
        if (ct.IsCancellationRequested)
          return;

        progress?.Report(handle.PercentComplete);
        await UniTask.Yield(PlayerLoopTiming.Update, ct);
      }

      if (handle.Status != AsyncOperationStatus.Succeeded)
        throw new Exception(
          $"[SceneLoader] Failed to load scene '{address}'. Status: {handle.Status}");

      progress?.Report(1f);
      Debug.Log($"[SceneLoader] '{address}' loaded successfully.");
    }

    #endregion

    #region Release

    /// <summary>
    /// No-op in LoadSceneMode.Single — Unity manages scene lifetime.
    /// Kept for interface compatibility.
    /// </summary>
    public void ReleaseCurrentScene()
    {
      _hasHandle = false;
    }

    #endregion
  }
}
