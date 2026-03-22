// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

using Code.View.Interfaces;
using Code.ViewModel.BottomSlots;

using R3;

using UnityEngine;

namespace Code.View
{
  /// <summary>
  /// MVVM View — spawns BottomSlotView prefabs and provides slot screen positions.
  ///
  /// Implements ISlotScreenPositionProvider so DragDropPresenter can ask
  /// "where is slot N on screen?" to aim the fly-back animation.
  ///
  /// Also triggers PlayBounce() on the target slot when an item lands in it,
  /// including the cancel/return-to-slot case (previously only OnDrop triggered it).
  /// </summary>
  public class BottomSlotsView : MonoBehaviour, ISlotScreenPositionProvider
  {
    [SerializeField] private RectTransform  _slotsRoot;
    [SerializeField] private BottomSlotView _slotPrefab;

    private readonly List<BottomSlotView> _slots = new();
    private CompositeDisposable _disposables;

    /// <summary>Called by UIFactory.</summary>
    public void Construct(IBottomSlotsViewModel slotsViewModel)
    {
      _disposables = new CompositeDisposable();

      for (int i = 0; i < slotsViewModel.SlotCount; i++)
      {
        var slot          = Instantiate(_slotPrefab, _slotsRoot);
        var slotViewModel = slotsViewModel.GetSlotViewModel(i);
        slot.SetViewModel(slotViewModel);
        _slots.Add(slot);

        // Play bounce on this slot whenever its content changes
        // (covers both direct OnDrop and the cancel-return path)
        int capturedIndex = i;
        slotViewModel.IsEmpty
          .Skip(1)                         // skip initial value
          .Where(empty => !empty)          // only when item arrives (empty→occupied)
          .Subscribe(_ => slot.PlayBounce())
          .AddTo(_disposables);
      }
    }

    private void OnDestroy() => _disposables?.Dispose();

    /// <inheritdoc/>
    public Vector2 GetSlotScreenPosition(int slotIndex)
    {
      if (slotIndex < 0 || slotIndex >= _slots.Count)
        return Vector2.zero;

      var rt = _slots[slotIndex].GetComponent<RectTransform>();
      return RectTransformUtility.WorldToScreenPoint(null, rt.position);
    }
  }
}
