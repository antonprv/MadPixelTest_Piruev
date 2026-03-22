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
  /// Loads scenes via Addressables + UniTask.
  ///
  /// Handle lifecycle:
  ///   Each LoadAsync stores the SceneInstance handle so the previous scene
  ///   can be properly released when a new one is loaded.
  ///   ReleaseCurrentScene() is only called when the handle was actually
  ///   acquired through Addressables — not for the initial bootstrap scene
  ///   which Unity loaded directly via Build Settings.
  ///
  /// "Already active" guard:
  ///   Compares scene name with SceneManager.GetActiveScene().name —
  ///   avoids double-loading the same scene (e.g. on hot restart).
  /// </summary>
  public class AddressableSceneLoader : ISceneLoader
  {
    private AsyncOperationHandle<SceneInstance> _currentHandle;
    private bool _hasAddressableHandle;

    public async UniTask LoadAsync(string address, CancellationToken ct = default)
    {
      if (SceneManager.GetActiveScene().name == address)
      {
        Debug.Log($"[SceneLoader] '{address}' is already active. Skipping.");
        return;
      }

      // Only release if the current scene was loaded by us via Addressables.
      // The very first scene (Initial) is loaded by Unity directly from
      // Build Settings — it has no Addressables handle, so releasing it
      // would throw "Unloading the last loaded scene is not supported".
      if (_hasAddressableHandle && _currentHandle.IsValid())
        Addressables.Release(_currentHandle);

      var handle = Addressables.LoadSceneAsync(address);
      await handle.ToUniTask(cancellationToken: ct);

      if (handle.Status != AsyncOperationStatus.Succeeded)
        throw new Exception($"[SceneLoader] Failed to load scene '{address}'. Status: {handle.Status}");

      _currentHandle = handle;
      _hasAddressableHandle = true;

      Debug.Log($"[SceneLoader] '{address}' loaded successfully.");
    }

    public void ReleaseCurrentScene()
    {
      if (_hasAddressableHandle && _currentHandle.IsValid())
        Addressables.Release(_currentHandle);

      _hasAddressableHandle = false;
    }
  }
}
