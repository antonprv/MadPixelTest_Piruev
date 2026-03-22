// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Core;
using Code.Data.StaticData;
using Code.Services.Interfaces;

using R3;

using Zenjex.Extensions.Lifecycle;

namespace Code.Services
{
  public class BottomSlotsService : IBottomSlotsService, IInitializable
  {
    private readonly Subject<int> _onSlotChanged = new();
    public Observable<int> OnSlotChanged => _onSlotChanged;

    private readonly BagConfig _bagConfig;
    private InventoryItem[] _slots;

    public int SlotCount => _bagConfig.BottomSlotCount;

    public BottomSlotsService(BagConfig bagConfig)
    {
      _bagConfig = bagConfig;
    }

    public void Initialize()
    {
      _slots = new InventoryItem[_bagConfig.BottomSlotCount];
    }

    // ─── Query ────────────────────────────────────────────────────────────────

    public InventoryItem GetSlot(int index) => _slots[index];
    public bool IsSlotEmpty(int index) => _slots[index] == null;

    public int FindFirstFreeSlot()
    {
      for (int i = 0; i < _slots.Length; i++)
        if (_slots[i] == null) return i;
      return -1;
    }

    // ─── Placement ────────────────────────────────────────────────────────────

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

      removed = _slots[slotIndex];
      _slots[slotIndex] = null;
      _onSlotChanged.OnNext(slotIndex);
      return true;
    }

    public bool TryPlaceInFirstFreeSlot(InventoryItem item, out int placedIndex)
    {
      placedIndex = FindFirstFreeSlot();
      if (placedIndex < 0) return false;
      return TryPlace(item, placedIndex);
    }
  }
}
