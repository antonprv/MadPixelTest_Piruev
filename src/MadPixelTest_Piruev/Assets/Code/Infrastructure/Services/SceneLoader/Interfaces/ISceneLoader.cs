// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

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

    /// <summary>Releases the handle of the currently loaded scene.</summary>
    void ReleaseCurrentScene();
  }
}
