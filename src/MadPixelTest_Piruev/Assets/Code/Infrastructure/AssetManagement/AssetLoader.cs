// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Code.Infrastructure.AssetManagement
{
  /// <summary>
  /// Wrapper over Addressables with handle cache.
  ///
  /// Cache pattern: on first LoadAsync starts operation and registers
  /// it in _completedHandles by key. Subsequent calls with same key
  /// return Result directly — without Addressables access.
  ///
  /// Cleanup() releases all handles (call on scene change / game end).
  /// </summary>
  public class AssetLoader : IAssetLoader
  {
    private readonly Dictionary<string, AsyncOperationHandle> _completedHandles = new();
    private readonly Dictionary<string, List<AsyncOperationHandle>> _calledHandles = new();

    // ─── Init ─────────────────────────────────────────────────────────────────

    public async UniTask InitializeAsync() =>
      await Addressables.InitializeAsync().ToUniTask();

    // ─── Load by AssetReference ───────────────────────────────────────────────

    public async UniTask<T> LoadAsync<T>(AssetReference reference) where T : Object
    {
      string key = reference.AssetGUID;

      if (TryGetCached<T>(key, out var cached))
        return cached;

      return await RunWithCache(key, Addressables.LoadAssetAsync<T>(reference));
    }

    // ─── Load by address string ───────────────────────────────────────────────

    public async UniTask<T> LoadAsync<T>(string address) where T : Object
    {
      if (TryGetCached<T>(address, out var cached))
        return cached;

      return await RunWithCache(address, Addressables.LoadAssetAsync<T>(address));
    }

    // ─── Cleanup ──────────────────────────────────────────────────────────────

    public void Cleanup()
    {
      foreach (var handleList in _calledHandles.Values)
        foreach (var handle in handleList)
          if (handle.IsValid())
            Addressables.Release(handle);

      _calledHandles.Clear();
      _completedHandles.Clear();
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private bool TryGetCached<T>(string key, out T result) where T : Object
    {
      if (_completedHandles.TryGetValue(key, out var handle) && handle.IsValid())
      {
        result = handle.Result as T;
        return result != null;
      }

      result = null;
      return false;
    }

    private async UniTask<T> RunWithCache<T>(string key, AsyncOperationHandle<T> op) where T : Object
    {
      // Register completed handle for subsequent cache-hit requests
      op.Completed += completed => _completedHandles[key] = completed;

      // Track for Cleanup
      if (!_calledHandles.TryGetValue(key, out var list))
      {
        list = new List<AsyncOperationHandle>();
        _calledHandles[key] = list;
      }
      list.Add(op);

      return await op.ToUniTask();
    }
  }
}
