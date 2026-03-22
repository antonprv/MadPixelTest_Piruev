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
  ///   ReleaseCurrentScene() should be called before loading a new scene
  ///   if you want to unload the old one from memory.
  ///
  /// "Already active" guard:
  ///   Compares scene name with SceneManager.GetActiveScene().name —
  ///   avoids double-loading the same scene (e.g. on hot restart).
  /// </summary>
  public class AddressableSceneLoader : ISceneLoader
  {
    private AsyncOperationHandle<SceneInstance> _currentHandle;

    public async UniTask LoadAsync(string address, CancellationToken ct = default)
    {
      // Already active — nothing to do
      if (SceneManager.GetActiveScene().name == address)
      {
        Debug.Log($"[SceneLoader] '{address}' is already active. Skipping.");
        return;
      }

      // Release previous handle before loading a new scene
      ReleaseCurrentScene();

      var handle = Addressables.LoadSceneAsync(address);

      // Await with cancellation support — if ct fires, the UniTask throws OperationCanceledException
      await handle.ToUniTask(cancellationToken: ct);

      if (handle.Status != AsyncOperationStatus.Succeeded)
        throw new Exception($"[SceneLoader] Failed to load scene '{address}'. Status: {handle.Status}");

      _currentHandle = handle;

      Debug.Log($"[SceneLoader] '{address}' loaded successfully.");
    }

    public void ReleaseCurrentScene()
    {
      if (_currentHandle.IsValid())
        Addressables.Release(_currentHandle);
    }
  }
}
