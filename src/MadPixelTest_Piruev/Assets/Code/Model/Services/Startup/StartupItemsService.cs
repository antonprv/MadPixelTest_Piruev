// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Data.StaticData;
using Code.Infrastructure.Services.StaticData.Interfaces;
using Code.Model.Core;
using Code.Model.Services.BottomSlots.Interfaces;
using Code.Model.Services.Inventory.Interfaces;

using UnityEngine;

namespace Code.Model.Services.Startup
{
  /// <summary>
  /// Places starter items into the inventory when a new session begins.
  ///
  /// Placement strategy:
  ///   For each ItemConfig in IItemDataSubservice.Items, tries all cells
  ///   in row-major order (left→right, top→bottom) until a valid origin is found.
  ///   Fills bottom slots with single-cell items first, then places remaining
  ///   items in the grid.
  ///
  /// This service is intentionally simple — it is a dev/test helper, not a
  /// save system. A real save system would replace this with loaded progress data.
  /// </summary>
  public class StartupItemsService : IStartupItemsService
  {
    private readonly IItemDataSubservice _itemData;
    private readonly IGridInventoryService _inventory;
    private readonly IBottomSlotsService _slots;
    private readonly IBagConfigSubservice _bagConfig;

    public StartupItemsService(
      IItemDataSubservice itemData,
      IGridInventoryService inventory,
      IBottomSlotsService slots,
      IBagConfigSubservice bagConfig)
    {
      _itemData = itemData;
      _inventory = inventory;
      _slots = slots;
      _bagConfig = bagConfig;
    }

    public void PlaceStartupItems()
    {
      int slotIndex = 0;

      foreach (var config in _itemData.Items)
      {
        // Try to fill a bottom slot first if the item fits in 1 cell
        if (IsSingleCell(config) && slotIndex < _bagConfig.BottomSlotCount)
        {
          var slotItem = new InventoryItem(config, Vector2Int.zero);
          if (_slots.TryPlace(slotItem, slotIndex))
          {
            slotIndex++;
            continue;
          }
        }

        // Otherwise place in the grid — scan row-major for a free origin
        TryPlaceInGrid(config);
      }
    }

    #region Helpers

    private void TryPlaceInGrid(ItemConfig config)
    {
      var grid = _bagConfig.GridSize;

      for (int y = 0; y < grid.y; y++)
        for (int x = 0; x < grid.x; x++)
        {
          var origin = new Vector2Int(x, y);

          if (!_inventory.CanPlace(config, origin))
            continue;

          var item = new InventoryItem(config, origin);
          _inventory.TryPlace(item);
          return;
        }
    }

    private static bool IsSingleCell(ItemConfig config) =>
      config.Shape != null && config.Shape.Count == 1;

    #endregion

  }
}
