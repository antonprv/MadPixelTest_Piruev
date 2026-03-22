// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Cysharp.Threading.Tasks;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Code.Infrastructure.AssetManagement
{
  public interface IAssetLoader
  {
    /// <summary>Initializes Addressables. Called once in BootstrapState.</summary>
    UniTask InitializeAsync();

    /// <summary>Loads asset by AssetReference. Caches handle — subsequent calls are instant.</summary>
    UniTask<T> LoadAsync<T>(AssetReference reference) where T : Object;

    /// <summary>Loads asset by string address. Caches handle.</summary>
    UniTask<T> LoadAsync<T>(string address) where T : Object;

    /// <summary>Releases all loaded handles. Called on level change.</summary>
    void Cleanup();
  }
}
