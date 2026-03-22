// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Model.Core;
using Code.Model.Services.BottomSlots.Interfaces;
using Code.Model.Services.DragDrop.Interfaces;
using Code.Model.Services.Inventory.Interfaces;
using Code.UI.Types;

using UnityEngine;

namespace Code.Model.Services.DragDrop
{
  /// <summary>
  /// Stores state of current drag operation.
  /// Contains no Unity UI logic — data only.
  ///
  /// Edge cases (handled via CancelDrag):
  ///  - Drop in void          → CancelDrag → return to slot or bag position
  ///  - Drop in invalid zone  → CancelDrag → same
  ///  - Slots full on bag exit → CancelDrag → return to bag
  /// </summary>
  public class GridDragDropService : IGridDragDropService
  {
    public bool IsDragging { get; private set; }
    public InventoryItem DraggedItem { get; private set; }
    public DragSource Source { get; private set; }
    public Vector2Int SourceOrigin { get; private set; }
    public int SourceSlotIndex { get; private set; }
    public Vector2Int DragOffset { get; private set; }

    private readonly IGridInventoryService _inventoryService;
    private readonly IBottomSlotsService _slotsService;

    public GridDragDropService(IGridInventoryService inventoryService, IBottomSlotsService slotsService)
    {
      _inventoryService = inventoryService;
      _slotsService = slotsService;
    }

    public void StartDrag(InventoryItem item, DragSource source, Vector2Int dragOffset, int sourceSlotIndex = -1)
    {
      IsDragging = true;
      DraggedItem = item;
      Source = source;
      SourceOrigin = item.Origin;
      SourceSlotIndex = sourceSlotIndex;
      DragOffset = dragOffset;
    }

    public void EndDrag()
    {
      IsDragging = false;
      DraggedItem = null;
      Source = DragSource.None;
      SourceOrigin = Vector2Int.zero;
      SourceSlotIndex = -1;
    }

    /// <summary>
    /// Returns item to original position.
    /// Order of attempts:
    ///   1. If source is slot: return to same slot
    ///   2. If source is grid: return to same grid position
    ///   3. Edge case: place in first free slot
    /// </summary>
    public void CancelDrag()
    {
      if (!IsDragging) return;

      bool returned = false;

      if (Source == DragSource.BottomSlot)
      {
        returned = _slotsService.TryPlace(DraggedItem, SourceSlotIndex);
      }
      else if (Source == DragSource.Bag)
      {
        DraggedItem.SetOrigin(SourceOrigin);
        returned = _inventoryService.TryPlace(DraggedItem);
      }

      // Edge case: slot or grid position already occupied
      // (should not happen with correct usage, but we safeguard)
      if (!returned)
      {
        if (!_slotsService.TryPlaceInFirstFreeSlot(DraggedItem, out _))
          UnityEngine.Debug.LogWarning("[GridDragDropService] Could not return item anywhere!");
      }

      EndDrag();
    }
  }
}
