using UnityEngine;
using BagFight.Core;
using BagFight.Services.Interfaces;
using BagFight.UI.Types;

namespace BagFight.Services
{
  /// <summary>
  /// Хранит состояние текущего drag-операции.
  /// Не содержит никакой Unity UI логики — только данные.
  ///
  /// Краевые случаи (обрабатываются через CancelDrag):
  ///  - Дроп в пустоту        → CancelDrag → возврат в слот или на место в сумке
  ///  - Дроп в невалидную зону → CancelDrag → то же
  ///  - Слоты полны при выносе из сумки → CancelDrag → возврат в сумку
  /// </summary>
  public class GridDragDropService : IGridDragDropService
  {
    public bool          IsDragging      { get; private set; }
    public InventoryItem DraggedItem     { get; private set; }
    public DragSource    Source          { get; private set; }
    public Vector2Int    SourceOrigin    { get; private set; }
    public int           SourceSlotIndex { get; private set; }
    public Vector2Int    DragOffset      { get; private set; }

    private readonly IGridInventoryService _inventoryService;
    private readonly IBottomSlotsService   _slotsService;

    public GridDragDropService(IGridInventoryService inventoryService, IBottomSlotsService slotsService)
    {
      _inventoryService = inventoryService;
      _slotsService     = slotsService;
    }

    public void StartDrag(InventoryItem item, DragSource source, Vector2Int dragOffset, int sourceSlotIndex = -1)
    {
      IsDragging      = true;
      DraggedItem     = item;
      Source          = source;
      SourceOrigin    = item.Origin;
      SourceSlotIndex = sourceSlotIndex;
      DragOffset      = dragOffset;
    }

    public void EndDrag()
    {
      IsDragging      = false;
      DraggedItem     = null;
      Source          = DragSource.None;
      SourceOrigin    = Vector2Int.zero;
      SourceSlotIndex = -1;
    }

    /// <summary>
    /// Возвращает предмет на исходное место.
    /// Порядок попыток:
    ///   1. Если источник — слот: вернуть в тот же слот
    ///   2. Если источник — грид: вернуть на ту же позицию в гриде
    ///   3. Крайний случай: положить в первый свободный слот
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

      // Крайний случай: слот или позиция в гриде уже заняты
      // (не должно происходить при корректном использовании, но страхуемся)
      if (!returned)
      {
        if (!_slotsService.TryPlaceInFirstFreeSlot(DraggedItem, out _))
          UnityEngine.Debug.LogWarning("[GridDragDropService] Could not return item anywhere!");
      }

      EndDrag();
    }
  }
}
