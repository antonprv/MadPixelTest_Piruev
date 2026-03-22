// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Infrastructure.Services.StaticData.Interfaces;
using Code.Model.Core;
using Code.Model.Services.BottomSlots.Interfaces;

using R3;

namespace Code.Model.Services.BottomSlots
{
  public class BottomSlotsService : IBottomSlotsService
  {
    private readonly Subject<int> _onSlotChanged = new();
    public Observable<int> OnSlotChanged => _onSlotChanged;

    private readonly IBagConfigSubservice _bagConfig;
    private InventoryItem[] _slots;

    public int SlotCount => _bagConfig.BottomSlotCount;

    public BottomSlotsService(IBagConfigSubservice bagConfig) =>
      _bagConfig = bagConfig;

    public void Initialize() =>
      _slots = new InventoryItem[_bagConfig.BottomSlotCount];

    #region Query

    public InventoryItem GetSlot(int index)          => _slots[index];
    public bool          IsSlotEmpty(int index)       => _slots[index] == null;

    public int FindFirstFreeSlot()
    {
      for (int i = 0; i < _slots.Length; i++)
        if (_slots[i] == null) return i;
      return -1;
    }

    #endregion

    #region Placement

    public bool TryPlace(InventoryItem item, int slotIndex)
    {
      if (slotIndex < 0 || slotIndex >= _slots.Length) return false;
      if (_slots[slotIndex] != null) return false;

      _slots[slotIndex] = item;
      _onSlotChanged.OnNext(slotIndex);
      return true;
    }

    public bool TryRemove(int slotIndex, out InventoryItem removed)
    {
      removed = null;
      if (slotIndex < 0 || slotIndex >= _slots.Length) return false;
      if (_slots[slotIndex] == null) return false;

      removed            = _slots[slotIndex];
      _slots[slotIndex]  = null;
      _onSlotChanged.OnNext(slotIndex);
      return true;
    }

    public bool TryPlaceInFirstFreeSlot(InventoryItem item, out int placedIndex)
    {
      placedIndex = FindFirstFreeSlot();
      if (placedIndex < 0) return false;
      return TryPlace(item, placedIndex);
    }

    #endregion
  }
}
