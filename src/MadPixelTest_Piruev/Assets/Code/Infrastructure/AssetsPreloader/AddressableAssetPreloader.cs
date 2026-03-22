// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System;
using System.Threading;

using Code.Common.Extensions.Logging;
using Code.Data.StaticData;
using Code.Infrastructure.AssetManagement;
using Code.Infrastructure.Services.StaticData.Interfaces;

using Cysharp.Threading.Tasks;

using R3;

using UnityEngine;

namespace Code.Infrastructure.AssetsPreloader
{
  /// <summary>
  /// Warms up Addressable icons of all items before gameplay starts.
  ///
  /// Depends on IItemDataSubservice (not ItemManifest directly) —
  /// static data loading and icon preloading are now separate responsibilities.
  /// IStaticDataService.LoadAllAsync() must be called before PreloadItemIconsAsync().
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

    private readonly IItemDataSubservice _itemData;
    private readonly IAssetLoader        _assetLoader;
    private readonly IGameLog            _logger;

    public AddressableAssetPreloader(
      IItemDataSubservice itemData,
      IAssetLoader        assetLoader,
      IGameLog            logger)
    {
      _itemData    = itemData;
      _assetLoader = assetLoader;
      _logger      = logger;
    }

    public async UniTask PreloadItemIconsAsync(IProgress<float> progress, CancellationToken ct)
    {
      var items = _itemData.Items;
      int total = items.Count;
      if (total == 0) return;

      int completed = 0;

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
        _logger.Log(
          LogType.Error,
          $"Failed to load icon for '{config.ItemId}': {e.Message}"
          );
      }
      finally
      {
        onLoaded();
      }
    }
  }
}
