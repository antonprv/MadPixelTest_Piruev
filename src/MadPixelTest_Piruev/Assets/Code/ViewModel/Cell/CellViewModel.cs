// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Infrastructure.AssetManagement;
using Code.Model.Core;
using Code.Presenter.Bag;
using Code.Presenter.DragDrop;
using Code.UI.Types;

using R3;

using UnityEngine;

namespace Code.ViewModel.Cell
{
  public interface ICellViewModel
  {
    #region Observable state

    /// <summary>Always the neutral empty-cell color.</summary>
    ReadOnlyReactiveProperty<Color> BackgroundColor { get; }

    /// <summary>
    /// Color of the item overlay (colored square on top of background).
    /// Color.clear when empty, item.Config.ItemColor when occupied.
    /// </summary>
    ReadOnlyReactiveProperty<Color> ItemOverlayColor { get; }

    ReadOnlyReactiveProperty<HighlightState> Highlight { get; }

    #endregion

    #region Commands

    void OnBeginDrag(Vector2 screenPosition);
    void OnDrag(Vector2 screenPosition);
    void OnEndDrag();
    void OnDrop();
    void OnPointerEnter();
    void OnPointerExit();

    #endregion

    void SetHighlight(HighlightState state);
    void Dispose();
  }

  /// <summary>
  /// MVVM ViewModel for a single inventory grid cell.
  ///
  /// Icon (sprite) is no longer managed here — BagView creates a single
  /// full-bounding-box Image per item that covers all its cells correctly.
  ///
  /// This VM only owns:
  ///   - BackgroundColor  — neutral, never changes
  ///   - ItemOverlayColor — item tint color, transparent when empty
  ///   - Highlight        — drag-hover state
  /// </summary>
  public class CellViewModel : ICellViewModel
  {
    private readonly Vector2Int         _coord;
    private readonly IBagPresenter      _bagPresenter;
    private readonly IDragDropPresenter _dragDropPresenter;

    private readonly ReactiveProperty<Color>          _backgroundColor;
    private readonly ReactiveProperty<Color>          _itemOverlayColor = new(Color.clear);
    private readonly ReactiveProperty<HighlightState> _highlight        = new(HighlightState.None);

    public ReadOnlyReactiveProperty<Color>          BackgroundColor  => _backgroundColor;
    public ReadOnlyReactiveProperty<Color>          ItemOverlayColor => _itemOverlayColor;
    public ReadOnlyReactiveProperty<HighlightState> Highlight        => _highlight;

    private readonly CompositeDisposable _disposables = new();
    private readonly Color               _emptyColor;

    public CellViewModel(
      Vector2Int         coord,
      IBagPresenter      bagPresenter,
      IDragDropPresenter dragDropPresenter,
      IAssetLoader       assetLoader,       // kept for interface compatibility
      Color              emptyColor)
    {
      _coord             = coord;
      _bagPresenter      = bagPresenter;
      _dragDropPresenter = dragDropPresenter;
      _emptyColor        = emptyColor;

      _backgroundColor = new ReactiveProperty<Color>(emptyColor);

      _bagPresenter.OnItemPlaced .Subscribe(_ => Refresh()).AddTo(_disposables);
      _bagPresenter.OnItemRemoved.Subscribe(_ => Refresh()).AddTo(_disposables);
      _bagPresenter.OnItemsMerged.Subscribe(_ => Refresh()).AddTo(_disposables);

      Refresh();
    }

    private void Refresh()
    {
      var item    = _bagPresenter.GetItemAt(_coord);
      bool isEmpty = item == null;
      _itemOverlayColor.Value = isEmpty ? Color.clear : item.Config.ItemColor;
    }

    #region Commands

    public void OnBeginDrag(Vector2 screenPosition) =>
      _dragDropPresenter.StartDragFromBag(_coord, screenPosition);

    public void OnDrag(Vector2 screenPosition) =>
      _dragDropPresenter.UpdateDragPosition(screenPosition);

    public void OnEndDrag() =>
      _dragDropPresenter.HandleEndDrag();

    public void OnDrop() =>
      _dragDropPresenter.HandleDropOnCell(_coord);

    public void OnPointerEnter() =>
      _dragDropPresenter.HandlePointerEnterCell(_coord);

    public void OnPointerExit() =>
      _dragDropPresenter.HandlePointerExitCell(_coord);

    #endregion

    public void SetHighlight(HighlightState state) =>
      _highlight.Value = state;

    public void Dispose() => _disposables.Dispose();
  }
}
