// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

using Code.Data.StaticData;
using Code.Model.Core;
using Code.Model.Services.Inventory.Interfaces;
using Code.UI.Types;

using R3;

using UnityEngine;

namespace Code.Presenter.Bag
{
  /// <summary>
  /// MVP Presenter — mediates between GridInventoryService (Model) and the View layer.
  ///
  /// Adds UI-facing concerns that don't belong in the Model:
  ///   - Highlight requests across multiple grid cells
  ///   - Delegates all domain operations to IGridInventoryService
  ///
  /// Registered as AsSingle in GameInstaller.
  /// </summary>
  public class BagPresenter : IBagPresenter
  {
    private readonly IGridInventoryService _inventoryService;
    private readonly Subject<HighlightRequest> _onHighlightRequested = new();

    public Observable<InventoryItem> OnItemPlaced => _inventoryService.OnItemPlaced;
    public Observable<InventoryItem> OnItemRemoved => _inventoryService.OnItemRemoved;
    public Observable<MergeResult> OnItemsMerged => _inventoryService.OnItemsMerged;
    public Observable<HighlightRequest> OnHighlightRequested => _onHighlightRequested;

    public BagPresenter(IGridInventoryService inventoryService)
    {
      _inventoryService = inventoryService;
    }

    #region Placement

    public bool CanPlace(ItemConfig config, Vector2Int origin, InventoryItem ignore = null)
      => _inventoryService.CanPlace(config, origin, ignore);

    public bool TryPlace(InventoryItem item)
      => _inventoryService.TryPlace(item);

    public bool TryRemove(InventoryItem item)
      => _inventoryService.TryRemove(item);

    #endregion

    #region Query

    public InventoryItem GetItemAt(Vector2Int cell) => _inventoryService.GetItemAt(cell);
    public IReadOnlyList<InventoryItem> GetAllItems() => _inventoryService.GetAllItems();
    #endregion

    #region Merge

    public bool CanMerge(InventoryItem dragged, Vector2Int targetCell, out InventoryItem targetItem)
      => _inventoryService.CanMerge(dragged, targetCell, out targetItem);

    public InventoryItem Merge(InventoryItem a, InventoryItem b)
      => _inventoryService.Merge(a, b);

    #endregion

    #region Highlight

    /// <summary>
    /// Called by DragDropPresenter on pointer enter/exit.
    /// Fires OnHighlightRequested → BagViewModel sets highlight on the correct CellViewModels.
    /// </summary>
    public void RequestHighlight(ItemConfig config, Vector2Int origin, HighlightState state)
      => _onHighlightRequested.OnNext(new HighlightRequest(config, origin, state));

    #endregion
  }
}
