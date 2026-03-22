// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

using Code.Data.StaticData.Configs;

using Code.Model.Core;
using Code.Model.Services.Inventory.Interfaces;
using Code.UI.Types;

using R3;

using UnityEngine;

namespace Code.Presenter.Bag
{
  /// <summary>
  /// MVP Presenter for the grid inventory.
  ///
  /// Sits between Model (GridInventory/GridInventoryService) and View layer.
  /// Exposes inventory operations + UI-facing events:
  ///   OnHighlightRequested — fires when drag hover changes highlight state across cells
  ///   OnMergeAnimation     — fires when a merge happens so BagViewModel triggers the effect
  ///
  /// ViewModels call this; Views never touch it directly.
  /// </summary>
  public interface IBagPresenter
  {
    #region Events (for ViewModels to subscribe)
    Observable<InventoryItem> OnItemPlaced { get; }
    Observable<InventoryItem> OnItemRemoved { get; }
    Observable<MergeResult> OnItemsMerged { get; }

    /// <summary>Fires when drag-hover changes highlight on a group of cells.</summary>
    Observable<HighlightRequest> OnHighlightRequested { get; }
    #endregion

    #region Placement
    bool CanPlace(ItemConfig config, Vector2Int origin, InventoryItem ignore = null);
    bool TryPlace(InventoryItem item);
    bool TryRemove(InventoryItem item);
    #endregion

    #region Query
    InventoryItem GetItemAt(Vector2Int cell);
    IReadOnlyList<InventoryItem> GetAllItems();
    #endregion

    #region Merge
    bool CanMerge(InventoryItem dragged, Vector2Int targetCell, out InventoryItem targetItem);
    InventoryItem Merge(InventoryItem a, InventoryItem b);
    #endregion

    #region Highlight (called by DragDropPresenter)
    void RequestHighlight(ItemConfig config, Vector2Int origin, HighlightState state);
    #endregion
  }

  /// <summary>Highlight request fired by presenter, consumed by BagViewModel.</summary>
  public readonly struct HighlightRequest
  {
    public readonly ItemConfig Config;
    public readonly Vector2Int Origin;
    public readonly HighlightState State;

    public HighlightRequest(ItemConfig config, Vector2Int origin, HighlightState state)
    {
      Config = config;
      Origin = origin;
      State = state;
    }
  }
}
