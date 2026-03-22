using System;
using Cysharp.Threading.Tasks;
using R3;

namespace BagFight.Infrastructure.AssetsPreloader
{
  public interface IAssetsPreloader
  {
    /// <summary>Прогресс загрузки [0..1]. Обновляется через R3 Observable.</summary>
    Observable<float> Progress { get; }

    /// <summary>
    /// Прогревает иконки всех предметов из ItemManifest параллельно.
    /// После завершения все AssetLoader.LoadAsync по тем же ссылкам мгновенны.
    /// </summary>
    UniTask PreloadItemIconsAsync(IProgress<float> progress, System.Threading.CancellationToken ct);
  }
}
