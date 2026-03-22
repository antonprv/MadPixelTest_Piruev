// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Data.StaticData.Configs;

using Code.Infrastructure.AssetManagement;
using Code.Infrastructure.Services.StaticData.Interfaces;
using Code.Model.Core;
using Code.Model.Services.BottomSlots.Interfaces;
using Code.Model.Services.Inventory.Interfaces;

using Cysharp.Threading.Tasks;

using UnityEngine;

namespace Code.Model.Services.Startup
{
  /// <summary>
  /// Places the level's starter items into the inventory at session start.
  ///
  /// Item source: ILevelStaticDataService.CurrentItemPreset.
  /// Each Entry holds an AssetReferenceT&lt;ItemConfig&gt; + Count.
  ///
  /// We load each ItemConfig via IAssetLoader (hits the cache — LevelStaticDataService
  /// already loaded them in LoadPresetItemConfigsAsync, so this is instant).
  /// We never read entry.Item.Asset directly because Addressables only populates
  /// that field when using its own LoadAssetAsync — our AssetLoader wrapper does
  /// not set it.
  ///
  /// PlaceStartupItems is async-void-safe: it fires PlaceStartupItemsAsync and
  /// lets GameLoopState continue. All placements finish before the first frame
  /// because the assets are already cached.
  /// </summary>
  public class StartupItemsService : IStartupItemsService
  {
    private readonly ILevelStaticDataService _levelData;
    private readonly IGridInventoryService   _inventory;
    private readonly IBottomSlotsService     _slots;
    private readonly IBagConfigSubservice    _bagConfig;
    private readonly IAssetLoader            _assetLoader;

    public StartupItemsService(
      ILevelStaticDataService levelData,
      IGridInventoryService   inventory,
      IBottomSlotsService     slots,
      IBagConfigSubservice    bagConfig,
      IAssetLoader            assetLoader)
    {
      _levelData   = levelData;
      _inventory   = inventory;
      _slots       = slots;
      _bagConfig   = bagConfig;
      _assetLoader = assetLoader;
    }

    public void PlaceStartupItems() => PlaceStartupItemsAsync().Forget();

    private async UniTaskVoid PlaceStartupItemsAsync()
    {
      var preset = _levelData.CurrentItemPreset;
      if (preset == null) return;

      int slotIndex = 0;

      foreach (var entry in preset.Items)
      {
        if (entry?.Item == null) continue;

        // Load via IAssetLoader — hits the in-memory cache instantly because
        // LevelStaticDataService.LoadPresetItemConfigsAsync already loaded these.
        var config = await _assetLoader.LoadAsync<ItemConfig>(entry.Item);

        if (config == null)
        {
          Debug.LogWarning(
            $"[StartupItemsService] Could not load ItemConfig for preset entry.");
          continue;
        }

        for (int i = 0; i < entry.Count; i++)
        {
          if (IsSingleCell(config) && slotIndex < _bagConfig.BottomSlotCount)
          {
            var slotItem = new InventoryItem(config, Vector2Int.zero);
            if (_slots.TryPlace(slotItem, slotIndex))
            {
              slotIndex++;
              continue;
            }
          }

          TryPlaceInGrid(config);
        }
      }
    }

    private void TryPlaceInGrid(ItemConfig config)
    {
      var grid = _bagConfig.GridSize;

      for (int y = 0; y < grid.y; y++)
        for (int x = 0; x < grid.x; x++)
        {
          var origin = new Vector2Int(x, y);
          if (!_inventory.CanPlace(config, origin)) continue;

          _inventory.TryPlace(new InventoryItem(config, origin));
          return;
        }

      Debug.LogWarning($"[StartupItemsService] No space for {config.ItemId}.");
    }

    private static bool IsSingleCell(ItemConfig config) =>
      config.Shape != null && config.Shape.Count == 1;
  }
}
