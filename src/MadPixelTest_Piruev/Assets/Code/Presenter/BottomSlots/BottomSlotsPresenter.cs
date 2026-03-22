// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Model.Core;
using Code.Model.Services.BottomSlots.Interfaces;

using R3;

namespace Code.Presenter.BottomSlots
{
  /// <summary>
  /// MVP Presenter for the bottom slots row.
  /// Thin wrapper over IBottomSlotsService — delegates all operations,
  /// exposes the same Observable events for ViewModels.
  /// </summary>
  public interface IBottomSlotsPresenter
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

  /// <summary>
  /// Delegates everything to IBottomSlotsService.
  /// Exists as a seam: if slot business rules grow (e.g. lock slots, validation),
  /// they live here — not in the service and not in the View.
  /// </summary>
  public class BottomSlotsPresenter : IBottomSlotsPresenter
  {
    private readonly IBottomSlotsService _slotsService;

    public int SlotCount => _slotsService.SlotCount;
    public Observable<int> OnSlotChanged => _slotsService.OnSlotChanged;

    public BottomSlotsPresenter(IBottomSlotsService slotsService)
    {
      _slotsService = slotsService;
    }

    public InventoryItem GetSlot(int index) => _slotsService.GetSlot(index);
    public bool IsSlotEmpty(int index) => _slotsService.IsSlotEmpty(index);
    public int FindFirstFreeSlot() => _slotsService.FindFirstFreeSlot();

    public bool TryPlace(InventoryItem item, int slotIndex)
      => _slotsService.TryPlace(item, slotIndex);

    public bool TryRemove(int slotIndex, out InventoryItem removed)
      => _slotsService.TryRemove(slotIndex, out removed);

    public bool TryPlaceInFirstFreeSlot(InventoryItem item, out int placedIndex)
      => _slotsService.TryPlaceInFirstFreeSlot(item, out placedIndex);
  }
}
