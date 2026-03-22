// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Infrastructure.AssetManagement;
using Code.Presenter.BottomSlots;
using Code.Presenter.DragDrop;

using Cysharp.Threading.Tasks;

using R3;

using UnityEngine;

namespace Code.ViewModel.BottomSlot
{
  public interface IBottomSlotViewModel
  {
    ReadOnlyReactiveProperty<bool> IsEmpty { get; }
    ReadOnlyReactiveProperty<Color> BackgroundColor { get; }
    ReadOnlyReactiveProperty<Sprite> Icon { get; }

    void OnBeginDrag(Vector2 screenPosition);
    void OnDrag(Vector2 screenPosition);
    void OnEndDrag();
    void OnDrop();

    void Dispose();
  }

  /// <summary>
  /// MVVM ViewModel for a single bottom slot.
  ///
  /// Subscribes to IBottomSlotsPresenter.OnSlotChanged for its own slot index
  /// and refreshes visual state when the slot's item changes.
  ///
  /// Lifecycle: created by BottomSlotsViewModel, disposed by it.
  /// </summary>
  public class BottomSlotViewModel : IBottomSlotViewModel
  {
    private readonly int _slotIndex;
    private readonly IBottomSlotsPresenter _slotsPresenter;
    private readonly IDragDropPresenter _dragDropPresenter;
    private readonly IAssetLoader _assetLoader;

    private readonly ReactiveProperty<bool> _isEmpty = new(true);
    private readonly ReactiveProperty<Color> _backgroundColor;
    private readonly ReactiveProperty<Sprite> _icon = new(null);

    public ReadOnlyReactiveProperty<bool> IsEmpty => _isEmpty;
    public ReadOnlyReactiveProperty<Color> BackgroundColor => _backgroundColor;
    public ReadOnlyReactiveProperty<Sprite> Icon => _icon;

    private readonly CompositeDisposable _disposables = new();

    private static readonly Color EmptyColor = new(0.12f, 0.12f, 0.12f, 0.5f);
    private static readonly Color OccupiedColor = new(0.25f, 0.25f, 0.25f, 0.8f);

    public BottomSlotViewModel(
      int slotIndex,
      IBottomSlotsPresenter slotsPresenter,
      IDragDropPresenter dragDropPresenter,
      IAssetLoader assetLoader)
    {
      _slotIndex = slotIndex;
      _slotsPresenter = slotsPresenter;
      _dragDropPresenter = dragDropPresenter;
      _assetLoader = assetLoader;

      _backgroundColor = new ReactiveProperty<Color>(EmptyColor);

      // Subscribe only to changes that affect THIS slot
      _slotsPresenter.OnSlotChanged
        .Where(idx => idx == _slotIndex)
        .Subscribe(_ => RefreshAsync().Forget())
        .AddTo(_disposables);

      RefreshAsync().Forget();
    }

    private async UniTaskVoid RefreshAsync()
    {
      var item = _slotsPresenter.GetSlot(_slotIndex);
      bool empty = item == null;

      _isEmpty.Value = empty;
      _backgroundColor.Value = empty ? EmptyColor : OccupiedColor;

      if (!empty && item.Config.Icon != null)
        _icon.Value = await _assetLoader.LoadAsync<Sprite>(item.Config.Icon);
      else
        _icon.Value = null;
    }

    #region Commands

    public void OnBeginDrag(Vector2 screenPosition)
      => _dragDropPresenter.StartDragFromSlot(_slotIndex, screenPosition);

    public void OnDrag(Vector2 screenPosition)
      => _dragDropPresenter.UpdateDragPosition(screenPosition);

    public void OnEndDrag()
      => _dragDropPresenter.HandleEndDrag();

    public void OnDrop()
      => _dragDropPresenter.HandleDropOnSlot(_slotIndex);

    public void Dispose() => _disposables.Dispose();
  }
  #endregion
}
