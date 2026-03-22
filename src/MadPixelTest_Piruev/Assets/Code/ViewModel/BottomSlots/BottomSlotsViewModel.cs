// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

using Code.Infrastructure.AssetManagement;
using Code.Infrastructure.Services.StaticData.Interfaces;
using Code.Presenter.BottomSlots;
using Code.Presenter.DragDrop;
using Code.ViewModel.BottomSlot;

namespace Code.ViewModel.BottomSlots
{
  public interface IBottomSlotsViewModel
  {
    int SlotCount { get; }
    IBottomSlotViewModel GetSlotViewModel(int index);
  }

  /// <summary>
  /// MVVM ViewModel for the bottom slots row.
  /// Creates and owns one BottomSlotViewModel per slot.
  /// BottomSlotsView queries this to get per-slot ViewModels.
  /// </summary>
  public class BottomSlotsViewModel : IBottomSlotsViewModel
  {
    private readonly List<BottomSlotViewModel> _slotViewModels = new();

    public int SlotCount => _slotViewModels.Count;

    public BottomSlotsViewModel(
      IBagConfigSubservice  bagConfig,
      IBottomSlotsPresenter slotsPresenter,
      IDragDropPresenter    dragDropPresenter,
      IAssetLoader          assetLoader)
    {
      for (int i = 0; i < bagConfig.BottomSlotCount; i++)
      {
        _slotViewModels.Add(new BottomSlotViewModel(
          i, slotsPresenter, dragDropPresenter, assetLoader));
      }
    }

    public IBottomSlotViewModel GetSlotViewModel(int index) => _slotViewModels[index];

    public void Dispose()
    {
      foreach (var vm in _slotViewModels)
        vm.Dispose();
      _slotViewModels.Clear();
    }
  }
}
