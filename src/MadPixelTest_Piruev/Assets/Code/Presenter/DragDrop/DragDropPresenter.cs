// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Infrastructure.AssetManagement;
using Code.Model.Services.DragDrop.Interfaces;
using Code.Presenter.Bag;
using Code.Presenter.BottomSlots;
using Code.UI.Types;
using Code.ViewModel.DragIcon;

using Cysharp.Threading.Tasks;

using UnityEngine;

namespace Code.Presenter.DragDrop
{
  /// <summary>
  /// Central MVP Presenter for drag-drop.
  ///
  /// All logic that previously lived in CellView.OnBeginDrag / OnDrop / OnEndDrag
  /// and in BottomSlotView.OnBeginDrag / OnDrop / OnEndDrag now lives here.
  ///
  /// Views become dumb: they forward raw Unity input events → this Presenter decides.
  ///
  /// Dependency chain (read-only):
  ///   DragDropPresenter
  ///     ├─ IBagPresenter        (for inventory operations + highlight requests)
  ///     ├─ IBottomSlotsPresenter (for slot operations)
  ///     ├─ IGridDragDropService  (for drag state tracking)
  ///     ├─ IDragIconViewModel    (to show/hide the floating drag icon)
  ///     └─ IAssetLoader         (to load icon sprites async)
  /// </summary>
  public class DragDropPresenter : IDragDropPresenter
  {
    private readonly IBagPresenter _bagPresenter;
    private readonly IBottomSlotsPresenter _slotsPresenter;
    private readonly IGridDragDropService _dragDropService;
    private readonly IDragIconViewModel _dragIconViewModel;
    private readonly IAssetLoader _assetLoader;

    public DragDropPresenter(
      IBagPresenter bagPresenter,
      IBottomSlotsPresenter slotsPresenter,
      IGridDragDropService dragDropService,
      IDragIconViewModel dragIconViewModel,
      IAssetLoader assetLoader)
    {
      _bagPresenter = bagPresenter;
      _slotsPresenter = slotsPresenter;
      _dragDropService = dragDropService;
      _dragIconViewModel = dragIconViewModel;
      _assetLoader = assetLoader;
    }

    #region Start

    public void StartDragFromBag(Vector2Int cellCoord, Vector2 screenPosition)
    {
      var item = _bagPresenter.GetItemAt(cellCoord);
      if (item == null) return;

      var dragOffset = cellCoord - item.Origin;
      _bagPresenter.TryRemove(item);
      _dragDropService.StartDrag(item, DragSource.Bag, dragOffset);

      ShowDragIconAsync(item.Config.Icon, screenPosition).Forget();
    }

    public void StartDragFromSlot(int slotIndex, Vector2 screenPosition)
    {
      if (!_slotsPresenter.TryRemove(slotIndex, out var item)) return;

      _dragDropService.StartDrag(item, DragSource.BottomSlot, Vector2Int.zero, slotIndex);

      ShowDragIconAsync(item.Config.Icon, screenPosition).Forget();
    }

    private async UniTaskVoid ShowDragIconAsync(
      UnityEngine.AddressableAssets.AssetReferenceSprite icon,
      Vector2 screenPosition)
    {
      var sprite = icon != null
        ? await _assetLoader.LoadAsync<Sprite>(icon)
        : null;

      if (_dragDropService.IsDragging && sprite != null)
        _dragIconViewModel.Show(sprite, screenPosition);
    }
    #endregion

    #region Move

    public void UpdateDragPosition(Vector2 screenPosition)
      => _dragIconViewModel.UpdatePosition(screenPosition);
    #endregion

    #region End

    /// <summary>
    /// Drop in void — behaviour differs by source:
    ///   Bag source   → try first free slot, else return to bag (CancelDrag)
    ///   Slot source  → always return to source slot (CancelDrag)
    /// </summary>
    public void HandleEndDrag()
    {
      _dragIconViewModel.Hide();

      if (!_dragDropService.IsDragging) return;

      if (_dragDropService.Source == DragSource.Bag)
      {
        var item = _dragDropService.DraggedItem;
        if (_slotsPresenter.TryPlaceInFirstFreeSlot(item, out _))
          _dragDropService.EndDrag();
        else
          _dragDropService.CancelDrag();
      }
      else // BottomSlot
      {
        _dragDropService.CancelDrag();
      }
    }
    #endregion

    #region Drop on cell

    public void HandleDropOnCell(Vector2Int cellCoord)
    {
      if (!_dragDropService.IsDragging) return;

      var dragged = _dragDropService.DraggedItem;
      var targetOrigin = cellCoord - _dragDropService.DragOffset;

      // 1. Merge?
      if (_bagPresenter.CanMerge(dragged, cellCoord, out var targetItem))
      {
        _bagPresenter.Merge(dragged, targetItem);
        _dragDropService.EndDrag();
        return;
      }

      // 2. Place?
      dragged.SetOrigin(targetOrigin);
      if (_bagPresenter.TryPlace(dragged))
      {
        _dragDropService.EndDrag();
        return;
      }

      // 3. Failed → return to source
      _dragDropService.CancelDrag();
    }
    #endregion

    #region Drop on slot

    public void HandleDropOnSlot(int slotIndex)
    {
      if (!_dragDropService.IsDragging) return;

      var dragged = _dragDropService.DraggedItem;
      var currentItem = _slotsPresenter.GetSlot(slotIndex);

      if (currentItem == null)
      {
        _slotsPresenter.TryPlace(dragged, slotIndex);
        _dragDropService.EndDrag();
      }
      else
      {
        // Try swap: displace old item to any free slot
        _slotsPresenter.TryRemove(slotIndex, out var displaced);

        if (_slotsPresenter.TryPlaceInFirstFreeSlot(displaced, out _))
        {
          _slotsPresenter.TryPlace(dragged, slotIndex);
          _dragDropService.EndDrag();
        }
        else
        {
          // No free slot — return displaced, cancel drag
          _slotsPresenter.TryPlace(displaced, slotIndex);
          _dragDropService.CancelDrag();
        }
      }
    }
    #endregion

    #region Highlight

    public void HandlePointerEnterCell(Vector2Int cellCoord)
    {
      if (!_dragDropService.IsDragging) return;

      var dragged = _dragDropService.DraggedItem;
      var targetOrigin = cellCoord - _dragDropService.DragOffset;

      HighlightState state;

      if (_bagPresenter.CanMerge(dragged, cellCoord, out _))
        state = HighlightState.Merge;
      else if (_bagPresenter.CanPlace(dragged.Config, targetOrigin, dragged))
        state = HighlightState.Valid;
      else
        state = HighlightState.Invalid;

      _bagPresenter.RequestHighlight(dragged.Config, targetOrigin, state);
    }

    public void HandlePointerExitCell(Vector2Int cellCoord)
    {
      if (!_dragDropService.IsDragging) return;

      var targetOrigin = cellCoord - _dragDropService.DragOffset;
      _bagPresenter.RequestHighlight(
        _dragDropService.DraggedItem.Config,
        targetOrigin,
        HighlightState.None);
    }
    #endregion
  }
}
