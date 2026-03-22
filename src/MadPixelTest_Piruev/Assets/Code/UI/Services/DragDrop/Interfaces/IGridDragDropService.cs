// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Core;
using Code.UI.Types;

using UnityEngine;

namespace Code.UI.Services.DragDrop.Interfaces
{
  public interface IGridDragDropService
  {
    bool IsDragging { get; }
    InventoryItem DraggedItem { get; }
    DragSource Source { get; }
    Vector2Int SourceOrigin { get; } // item origin BEFORE drag start
    int SourceSlotIndex { get; } // -1 if source is grid

    /// <summary>
    /// Offset from grabbed cell to item origin.
    /// Example: dragging L-shape by bottom cell (offset (0,2)) —
    /// origin = hoveredCell - DragOffset.
    /// </summary>
    Vector2Int DragOffset { get; }

    void StartDrag(InventoryItem item, DragSource source, Vector2Int dragOffset, int sourceSlotIndex = -1);
    void EndDrag();

    /// <summary>
    /// Cancels drag and returns item to original position.
    /// Called when item is dropped in invalid position.
    /// </summary>
    void CancelDrag();
  }
}
