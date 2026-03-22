// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System;
using System.Threading;

using Code.Data.StaticData;
using Code.Infrastructure.AssetManagement;

using Cysharp.Threading.Tasks;

using R3;

using UnityEngine;

namespace Code.Infrastructure.AssetsPreloader
{
  /// <summary>
  /// Warms up Addressable icons of all items from ItemManifest before gameplay starts.
  ///
  /// Parallel loading pattern:
  ///   Create array of UniTasks — one task per ItemConfig.
  ///   UniTask.WhenAll runs them all in parallel.
  ///   After WhenAll, each AssetLoader.LoadAsync<Sprite>(icon) returns
  ///   result from cache synchronously — without gameplay stop.
  ///
  /// Progress via IProgress<float>:
  ///   Each completed task increments counter and pushes new value
  ///   to _progressSubject → LoadingCurtain subscribes and updates progress bar.
  /// </summary>
  public class AddressableAssetPreloader : IAssetsPreloader
  {
    private readonly Subject<float> _progressSubject = new();
    public Observable<float> Progress => _progressSubject;

    private readonly ItemManifest _manifest;
    private readonly IAssetLoader _assetLoader;

    public AddressableAssetPreloader(ItemManifest manifest, IAssetLoader assetLoader)
    {
      _manifest = manifest;
      _assetLoader = assetLoader;
    }

    public async UniTask PreloadItemIconsAsync(IProgress<float> progress, CancellationToken ct)
    {
      var items = _manifest.Items;
      int total = items.Count;
      if (total == 0) return;

      int completed = 0;

      // Create tasks for each item that has Icon defined
      var tasks = new UniTask[total];
      for (int i = 0; i < total; i++)
      {
        var config = items[i];
        tasks[i] = LoadIconAsync(config, () =>
        {
          completed++;
          float ratio = (float)completed / total;
          progress?.Report(ratio);
          _progressSubject.OnNext(ratio);
        }, ct);
      }

      // Parallel loading — all icons load simultaneously
      await UniTask.WhenAll(tasks);

      _progressSubject.OnNext(1f);
    }

    private async UniTask LoadIconAsync(ItemConfig config, Action onLoaded, CancellationToken ct)
    {
      if (ct.IsCancellationRequested) return;
      if (config.Icon == null) { onLoaded(); return; }

      try
      {
        await _assetLoader.LoadAsync<Sprite>(config.Icon);
      }
      catch (Exception e)
      {
        Debug.LogError($"[BagAssetsPreloader] Failed to load icon for '{config.ItemId}': {e.Message}");
      }
      finally
      {
        onLoaded();
      }
    }
  }
}
