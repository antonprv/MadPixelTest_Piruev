// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System;
using System.Threading;

using Cysharp.Threading.Tasks;

namespace Code.Infrastructure.SceneLoader
{
  public interface ISceneLoader
  {
    /// <summary>
    /// Loads an Addressable scene by address.
    /// If the scene is already active — returns immediately without reloading.
    /// </summary>
    UniTask LoadAsync(string address, CancellationToken ct = default);

    /// <summary>
    /// Same as above but reports load progress [0..1] via IProgress.
    /// Used by LoadLevelState to update the loading curtain.
    /// </summary>
    UniTask LoadAsync(string address, CancellationToken ct, IProgress<float> progress);

    /// <summary>Releases the handle of the currently loaded Addressable scene.</summary>
    void ReleaseCurrentScene();
  }
}
