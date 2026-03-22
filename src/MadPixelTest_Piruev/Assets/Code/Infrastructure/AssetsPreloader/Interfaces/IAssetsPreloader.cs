// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System;

using Cysharp.Threading.Tasks;

using R3;

namespace Code.Infrastructure.AssetsPreloader
{
  public interface IAssetsPreloader
  {
    /// <summary>Loading progress [0..1]. Updated via R3 Observable.</summary>
    Observable<float> Progress { get; }

    /// <summary>
    /// Warms up icons of all items from ItemManifest in parallel.
    /// After completion, all AssetLoader.LoadAsync calls for same addresses are instant.
    /// </summary>
    UniTask PreloadItemIconsAsync(IProgress<float> progress, System.Threading.CancellationToken ct);
  }
}
