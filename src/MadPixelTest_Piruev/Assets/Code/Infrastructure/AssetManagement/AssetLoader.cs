using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace BagFight.Infrastructure.AssetManagement
{
  /// <summary>
  /// Обёртка над Addressables с кэшем хэндлов.
  ///
  /// Паттерн кэша: при первом LoadAsync запускает операцию и регистрирует
  /// её в _completedHandles по ключу. Повторный вызов с тем же ключом
  /// возвращает Result напрямую — без обращения к Addressables.
  ///
  /// Cleanup() освобождает все хэндлы (вызывать при смене сцены / завершении игры).
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
      // Регистрируем completed-хэндл для последующих cache-hit запросов
      op.Completed += completed => _completedHandles[key] = completed;

      // Трекаем для Cleanup
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
