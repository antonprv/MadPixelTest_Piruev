using System.Collections.Generic;
using R3;
using UnityEngine;
using BagFight.Core;
using BagFight.Data;
using BagFight.Services.Interfaces;
using Zenjex.Extensions.Lifecycle;

namespace BagFight.Services
{
  public class GridInventoryService : IGridInventoryService, IInitializable
  {
    // ─── Events ───────────────────────────────────────────────────────────────
    private readonly Subject<InventoryItem> _onItemPlaced  = new();
    private readonly Subject<InventoryItem> _onItemRemoved = new();
    private readonly Subject<MergeResult>   _onItemsMerged = new();

    public Observable<InventoryItem> OnItemPlaced  => _onItemPlaced;
    public Observable<InventoryItem> OnItemRemoved => _onItemRemoved;
    public Observable<MergeResult>   OnItemsMerged => _onItemsMerged;

    // ─── Dependencies ─────────────────────────────────────────────────────────
    private readonly BagConfig _bagConfig;
    private GridInventory      _grid;

    public GridInventoryService(BagConfig bagConfig)
    {
      _bagConfig = bagConfig;
    }

    // IInitializable — вызывается Zenjex после сборки контейнера
    public void Initialize()
    {
      _grid = new GridInventory(_bagConfig.GetActiveCellsSet());
    }

    // ─── Placement ────────────────────────────────────────────────────────────

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

    // ─── Query ────────────────────────────────────────────────────────────────

    public InventoryItem              GetItemAt(Vector2Int cell) => _grid.GetItemAt(cell);
    public IReadOnlyList<InventoryItem> GetAllItems()            => _grid.Items;

    // ─── Merge ────────────────────────────────────────────────────────────────

    public bool CanMerge(InventoryItem dragged, Vector2Int targetCell, out InventoryItem targetItem)
      => _grid.CanMerge(dragged, targetCell, out targetItem);

    public InventoryItem Merge(InventoryItem a, InventoryItem b)
    {
      var merged = _grid.Merge(a, b);
      _onItemsMerged.OnNext(new MergeResult(a, b, merged));
      return merged;
    }
  }
}
