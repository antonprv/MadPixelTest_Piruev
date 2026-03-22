// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Core;

using R3;

namespace Code.UI.Services.BottomSlots.Interfaces
{
  public interface IBottomSlotsService
  {
    int SlotCount { get; }

    Observable<int> OnSlotChanged { get; }

    InventoryItem GetSlot(int index);
    bool IsSlotEmpty(int index);
    int FindFirstFreeSlot();

    bool TryPlace(InventoryItem item, int slotIndex);
    bool TryRemove(int slotIndex, out InventoryItem removed);
    bool TryPlaceInFirstFreeSlot(InventoryItem item, out int placedIndex);
  }
}
