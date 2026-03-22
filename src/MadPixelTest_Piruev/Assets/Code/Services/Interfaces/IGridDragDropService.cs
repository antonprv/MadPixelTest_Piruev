using UnityEngine;
using BagFight.Core;
using BagFight.UI.Types;

namespace BagFight.Services.Interfaces
{
  public interface IGridDragDropService
  {
    bool          IsDragging      { get; }
    InventoryItem DraggedItem     { get; }
    DragSource    Source          { get; }
    Vector2Int    SourceOrigin    { get; } // origin предмета ДО начала драга
    int           SourceSlotIndex { get; } // -1 если источник — грид

    /// <summary>
    /// Смещение от захваченной ячейки до origin предмета.
    /// Пример: тащим L-шку за нижнюю ячейку (offset (0,2)) —
    /// origin = hoveredCell - DragOffset.
    /// </summary>
    Vector2Int DragOffset { get; }

    void StartDrag(InventoryItem item, DragSource source, Vector2Int dragOffset, int sourceSlotIndex = -1);
    void EndDrag();

    /// <summary>
    /// Отменяет драг и возвращает предмет на исходное место.
    /// Вызывается, когда предмет дропнули в недопустимую позицию.
    /// </summary>
    void CancelDrag();
  }
}
