// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

using Code.Data.StaticData;
using Code.Infrastructure.AssetManagement;
using Code.Infrastructure.AssetManagement.Addresses;
using Code.Infrastructure.Services.StaticData.Interfaces;

using Cysharp.Threading.Tasks;

using UnityEngine.AddressableAssets;

namespace Code.Infrastructure.Services.StaticData.Subservices
{
  /// <summary>
  /// Loads ItemManifest from Addressables, then resolves each ItemConfig
  /// lazily on first request via IAssetLoader.
  ///
  /// IAssetLoader caches by GUID — repeated ForItemAsync calls with the same
  /// id return the cached instance without a second Addressables round-trip.
  ///
  /// The Items list is populated after LoadSelfAsync completes so that
  /// AddressableAssetPreloader can iterate it for icon warm-up.
  /// </summary>
  public class ItemDataSubservice : IItemDataSubservice
  {
    public IReadOnlyList<ItemConfig> Items => _loadedItems;

    private readonly List<ItemConfig> _loadedItems = new();
    private readonly Dictionary<string, ItemConfig> _itemDatas = new();

    private ItemManifest _manifest;

    private readonly IAssetLoader _assetLoader;

    public ItemDataSubservice(IAssetLoader assetLoader) =>
      _assetLoader = assetLoader;

    /// <summary>
    /// Loads the manifest and eagerly resolves every ItemConfig so that
    /// AddressableAssetPreloader can access Items immediately after this call.
    /// </summary>
    public async UniTask LoadSelfAsync()
    {
      if (_manifest != null) return;

      _manifest = await _assetLoader
        .LoadAsync<ItemManifest>(StaticDataAddresses.ItemManifest);

      // Resolve all configs in parallel — IAssetLoader caches by GUID
      var tasks = new List<UniTask>(_manifest.Items.Count);
      foreach (var (id, reference) in _manifest.Items)
        tasks.Add(LoadAndRegisterAsync(id, reference));

      await UniTask.WhenAll(tasks);
    }

    /// <inheritdoc/>
    public ItemConfig ForItem(string itemId) =>
      _itemDatas.TryGetValue(itemId, out var cfg) ? cfg : null;

    private async UniTask LoadAndRegisterAsync(string id, AssetReferenceT<ItemConfig> reference)
    {
      var cfg = await _assetLoader.LoadAsync<ItemConfig>(reference);
      if (cfg == null) return;

      _itemDatas[id]    = cfg;
      _loadedItems.Add(cfg);
    }
  }
}
