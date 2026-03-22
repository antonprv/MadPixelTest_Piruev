// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Code.ViewModel.BottomSlots;

using UnityEngine;

namespace Code.View
{
  /// <summary>
  /// MVVM View — spawns BottomSlotView prefabs and assigns ViewModels.
  /// Receives its ViewModel via Construct() called by UIFactory.
  /// </summary>
  public class BottomSlotsView : MonoBehaviour
  {
    [SerializeField] private RectTransform  _slotsRoot;
    [SerializeField] private BottomSlotView _slotPrefab;

    /// <summary>Called by UIFactory after domain services are initialized.</summary>
    public void Construct(IBottomSlotsViewModel slotsViewModel)
    {
      for (int i = 0; i < slotsViewModel.SlotCount; i++)
      {
        var slot          = Instantiate(_slotPrefab, _slotsRoot);
        var slotViewModel = slotsViewModel.GetSlotViewModel(i);
        slot.SetViewModel(slotViewModel);
      }
    }
  }
}
