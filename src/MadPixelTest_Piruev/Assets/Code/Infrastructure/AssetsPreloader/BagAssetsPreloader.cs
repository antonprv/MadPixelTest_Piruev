using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using BagFight.Data;
using BagFight.Infrastructure.AssetManagement;

namespace BagFight.Infrastructure.AssetsPreloader
{
  /// <summary>
  /// Прогревает Addressable-иконки всех предметов из ItemManifest до начала геймплея.
  ///
  /// Паттерн параллельной загрузки:
  ///   Создаём массив UniTask — по одной задаче на каждый ItemConfig.
  ///   UniTask.WhenAll запускает их все параллельно.
  ///   После WhenAll каждый AssetLoader.LoadAsync<Sprite>(icon) возвращает
  ///   результат из кэша синхронно — без стопа геймплея.
  ///
  /// Прогресс через IProgress<float>:
  ///   Каждая завершённая задача инкрементирует счётчик и пушит новое значение
  ///   в _progressSubject → LoadingCurtain подписывается и обновляет полосу.
  /// </summary>
  public class BagAssetsPreloader : IAssetsPreloader
  {
    private readonly Subject<float> _progressSubject = new();
    public Observable<float> Progress => _progressSubject;

    private readonly ItemManifest _manifest;
    private readonly IAssetLoader _assetLoader;

    public BagAssetsPreloader(ItemManifest manifest, IAssetLoader assetLoader)
    {
      _manifest    = manifest;
      _assetLoader = assetLoader;
    }

    public async UniTask PreloadItemIconsAsync(IProgress<float> progress, CancellationToken ct)
    {
      var items = _manifest.Items;
      int total = items.Count;
      if (total == 0) return;

      int completed = 0;

      // Создаём задачи на каждый предмет, у которого задан Icon
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

      // Параллельная загрузка — все иконки грузятся одновременно
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
