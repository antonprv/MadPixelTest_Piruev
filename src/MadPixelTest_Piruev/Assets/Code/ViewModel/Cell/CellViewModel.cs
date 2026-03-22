// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Infrastructure.AssetManagement;
using Code.Model.Core;
using Code.Presenter.Bag;
using Code.Presenter.DragDrop;
using Code.UI.Types;

using Cysharp.Threading.Tasks;

using R3;

using UnityEngine;

namespace Code.ViewModel.Cell
{
  public interface ICellViewModel
  {
    #region Observable state (View binds to these)

    ReadOnlyReactiveProperty<Color> BackgroundColor { get; }
    ReadOnlyReactiveProperty<Sprite> Icon { get; }
    ReadOnlyReactiveProperty<bool> IconVisible { get; }
    ReadOnlyReactiveProperty<HighlightState> Highlight { get; }

    #endregion

    #region Commands (View forwards Unity input events here)

    void OnBeginDrag(Vector2 screenPosition);
    void OnDrag(Vector2 screenPosition);
    void OnEndDrag();
    void OnDrop();
    void OnPointerEnter();
    void OnPointerExit();

    #endregion

    #region Called by BagViewModel to set highlight from HighlightRequest

    void SetHighlight(HighlightState state);

    #endregion

    void Dispose();
  }

  /// <summary>
  /// MVVM ViewModel for a single inventory grid cell.
  ///
  /// Owns all visual state as ReactiveProperties.
  /// Delegates input commands to DragDropPresenter.
  /// Refreshes state by querying IBagPresenter when inventory events fire.
  ///
  /// Lifecycle: created by BagViewModel, disposed by BagViewModel.
  /// Never registered in DI — created via new CellViewModel(...).
  /// </summary>
  public class CellViewModel : ICellViewModel
  {
    private readonly Vector2Int _coord;
    private readonly IBagPresenter _bagPresenter;
    private readonly IDragDropPresenter _dragDropPresenter;
    private readonly IAssetLoader _assetLoader;

    #region State

    private readonly ReactiveProperty<Color> _backgroundColor;
    private readonly ReactiveProperty<Sprite> _icon = new(null);
    private readonly ReactiveProperty<bool> _iconVisible = new(false);
    private readonly ReactiveProperty<HighlightState> _highlight = new(HighlightState.None);

    #endregion

    public ReadOnlyReactiveProperty<Color> BackgroundColor => _backgroundColor;
    public ReadOnlyReactiveProperty<Sprite> Icon => _icon;
    public ReadOnlyReactiveProperty<bool> IconVisible => _iconVisible;
    public ReadOnlyReactiveProperty<HighlightState> Highlight => _highlight;

    private readonly CompositeDisposable _disposables = new();

    // Inspector-configurable defaults exposed via ctor so BagViewModel can pass them
    private readonly Color _emptyColor;

    public CellViewModel(
      Vector2Int coord,
      IBagPresenter bagPresenter,
      IDragDropPresenter dragDropPresenter,
      IAssetLoader assetLoader,
      Color emptyColor)
    {
      _coord = coord;
      _bagPresenter = bagPresenter;
      _dragDropPresenter = dragDropPresenter;
      _assetLoader = assetLoader;
      _emptyColor = emptyColor;

      _backgroundColor = new ReactiveProperty<Color>(emptyColor);

      // Subscribe to inventory events → refresh this cell's visual state
      _bagPresenter.OnItemPlaced
        .Subscribe(_ => Refresh())
        .AddTo(_disposables);

      _bagPresenter.OnItemRemoved
        .Subscribe(_ => Refresh())
        .AddTo(_disposables);

      _bagPresenter.OnItemsMerged
        .Subscribe(_ => Refresh())
        .AddTo(_disposables);

      Refresh();
    }

    #region Refresh

    /// <summary>
    /// Synchronously updates background color and icon visibility.
    /// Icon asset loading is fire-and-forget async — it doesn't block
    /// reactive property updates that tests need to observe immediately.
    /// </summary>
    private void Refresh()
    {
      var item = _bagPresenter.GetItemAt(_coord);
      bool isEmpty = item == null;

      _backgroundColor.Value = isEmpty ? _emptyColor : item.Config.ItemColor;

      bool isOrigin = !isEmpty && item.Origin == _coord;
      _iconVisible.Value = isOrigin;

      if (!isOrigin)
        _icon.Value = null;
      else if (item.Config.Icon != null)
        LoadIconAsync(item).Forget();
    }

    private async UniTaskVoid LoadIconAsync(InventoryItem item) =>
      _icon.Value = await _assetLoader.LoadAsync<Sprite>(item.Config.Icon);

    #endregion

    #region Commands

    public void OnBeginDrag(Vector2 screenPosition)
      => _dragDropPresenter.StartDragFromBag(_coord, screenPosition);

    public void OnDrag(Vector2 screenPosition)
      => _dragDropPresenter.UpdateDragPosition(screenPosition);

    public void OnEndDrag()
      => _dragDropPresenter.HandleEndDrag();

    public void OnDrop()
      => _dragDropPresenter.HandleDropOnCell(_coord);

    public void OnPointerEnter()
      => _dragDropPresenter.HandlePointerEnterCell(_coord);

    public void OnPointerExit()
      => _dragDropPresenter.HandlePointerExitCell(_coord);

    #endregion

    #region Highlight (set externally by BagViewModel)

    public void SetHighlight(HighlightState state)
      => _highlight.Value = state;

    #endregion

    public void Dispose() => _disposables.Dispose();
  }
}
