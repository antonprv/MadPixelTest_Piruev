using R3;
using BagFight.Core;

namespace BagFight.Services.Interfaces
{
  public interface IBottomSlotsService
  {
    int SlotCount { get; }

    Observable<int> OnSlotChanged { get; }

    InventoryItem GetSlot(int index);
    bool          IsSlotEmpty(int index);
    int           FindFirstFreeSlot();

    bool TryPlace(InventoryItem item, int slotIndex);
    bool TryRemove(int slotIndex, out InventoryItem removed);
    bool TryPlaceInFirstFreeSlot(InventoryItem item, out int placedIndex);
  }
}
