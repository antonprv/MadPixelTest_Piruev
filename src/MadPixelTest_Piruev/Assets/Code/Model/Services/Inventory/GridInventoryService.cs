// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

using Code.Data.StaticData;
using Code.Infrastructure.Services.StaticData.Interfaces;
using Code.Model.Core;
using Code.Model.Services.Inventory.Interfaces;

using R3;

using UnityEngine;

using Zenjex.Extensions.Lifecycle;

namespace Code.Model.Services.Inventory
{
  public class GridInventoryService : IGridInventoryService, IInitializable
  {
    #region Events

    private readonly Subject<InventoryItem> _onItemPlaced  = new();
    private readonly Subject<InventoryItem> _onItemRemoved = new();
    private readonly Subject<MergeResult>   _onItemsMerged = new();

    public Observable<InventoryItem> OnItemPlaced  => _onItemPlaced;
    public Observable<InventoryItem> OnItemRemoved => _onItemRemoved;
    public Observable<MergeResult>   OnItemsMerged => _onItemsMerged;

    #endregion

    #region Dependencies

    private readonly IBagConfigSubservice _bagConfig;
    private GridInventory _grid;

    public GridInventoryService(IBagConfigSubservice bagConfig) =>
      _bagConfig = bagConfig;

    // IInitializable — called by Zenjex after container assembly
    public void Initialize() =>
      _grid = new GridInventory(_bagConfig.GetActiveCellsSet());

    #endregion

    #region Placement

    public bool CanPlace(ItemConfig config, Vector2Int origin, InventoryItem ignore = null)
      => _grid.CanPlace(config, origin, ignore);

    public bool TryPlace(InventoryItem item)
    {
      if (!_grid.TryPlace(item))
        return false;

      _onItemPlaced.OnNext(item);
      return true;
    }

    public bool TryRemove(InventoryItem item)
    {
      if (!_grid.TryRemove(item))
        return false;

      _onItemRemoved.OnNext(item);
      return true;
    }

    #endregion

    #region Query

    public InventoryItem GetItemAt(Vector2Int cell) => _grid.GetItemAt(cell);
    public IReadOnlyList<InventoryItem> GetAllItems() => _grid.Items;

    #endregion

    #region Merge

    public bool CanMerge(InventoryItem dragged, Vector2Int targetCell, out InventoryItem targetItem)
      => _grid.CanMerge(dragged, targetCell, out targetItem);

    public InventoryItem Merge(InventoryItem a, InventoryItem b)
    {
      var merged = _grid.Merge(a, b);
      _onItemsMerged.OnNext(new MergeResult(a, b, merged));
      return merged;
    }

    #endregion
  }
}
