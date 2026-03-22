// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Code.ViewModel.BottomSlots;

using UnityEngine;

using Zenjex.Extensions.Attribute;
using Zenjex.Extensions.Injector;

namespace Code.View
{
  /// <summary>
  /// MVVM View — spawns BottomSlotView prefabs and assigns ViewModels from IBottomSlotsViewModel.
  /// </summary>
  public class BottomSlotsView : ZenjexBehaviour
  {
    [SerializeField] private RectTransform _slotsRoot;
    [SerializeField] private BottomSlotView _slotPrefab;

    [Zenjex] private IBottomSlotsViewModel _slotsViewModel;

    protected override void OnAwake()
    {
      for (int i = 0; i < _slotsViewModel.SlotCount; i++)
      {
        var slot = Instantiate(_slotPrefab, _slotsRoot);
        var slotViewModel = _slotsViewModel.GetSlotViewModel(i);
        slot.SetViewModel(slotViewModel);
      }
    }
  }
}
