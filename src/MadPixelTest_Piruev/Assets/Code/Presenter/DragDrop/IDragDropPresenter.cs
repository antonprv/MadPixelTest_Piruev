// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using UnityEngine;

namespace Code.Presenter.DragDrop
{
  /// <summary>
  /// MVP Presenter that owns ALL drag-drop business logic.
  ///
  /// Previously this logic was scattered across CellView and BottomSlotView.
  /// Now Views just forward raw input events here; the Presenter decides what to do.
  ///
  /// Responsibilities:
  ///   - StartDragFromBag / StartDragFromSlot  → removes item from source, starts drag state
  ///   - UpdateDragPosition                    → moves the drag icon
  ///   - HandleDropOnCell / HandleDropOnSlot   → merge/place/swap/cancel logic
  ///   - HandleEndDrag                         → void-drop fallback logic
  ///   - HandlePointerEnterCell / Exit         → highlight requests
  /// </summary>
  public interface IDragDropPresenter
  {
    /// <summary>Drag started by grabbing a grid cell.</summary>
    void StartDragFromBag(Vector2Int cellCoord, Vector2 screenPosition);

    /// <summary>Drag started by grabbing a bottom slot.</summary>
    void StartDragFromSlot(int slotIndex, Vector2 screenPosition);

    /// <summary>Pointer moved during drag — update floating icon position.</summary>
    void UpdateDragPosition(Vector2 screenPosition);

    /// <summary>
    /// Drag ended without a valid drop target (pointer released in void).
    /// Tries to place in a free slot; otherwise cancels and returns to source.
    /// </summary>
    void HandleEndDrag();

    /// <summary>Item dropped onto a grid cell — try merge, then try place, then cancel.</summary>
    void HandleDropOnCell(Vector2Int cellCoord);

    /// <summary>Item dropped onto a bottom slot — place or swap.</summary>
    void HandleDropOnSlot(int slotIndex);

    /// <summary>Pointer entered a cell during drag — request highlight state update.</summary>
    void HandlePointerEnterCell(Vector2Int cellCoord);

    /// <summary>Pointer left a cell during drag — clear highlight.</summary>
    void HandlePointerExitCell(Vector2Int cellCoord);
  }
}
