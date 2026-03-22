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
  /// Highlight fix:
  ///   Stores _lastHighlightOrigin so that HandlePointerEnterCell always clears
  ///   the previous shape highlight before applying the new one. This prevents
  ///   "ghost" highlights when the pointer moves quickly between cells without
  ///   triggering PointerExit on every intermediate cell.
  /// </summary>
  public class DragDropPresenter : IDragDropPresenter
  {
    private readonly IBagPresenter         _bagPresenter;
    private readonly IBottomSlotsPresenter _slotsPresenter;
    private readonly IGridDragDropService  _dragDropService;
    private readonly IDragIconViewModel    _dragIconViewModel;
    private readonly IAssetLoader          _assetLoader;

    // Tracks the last origin we highlighted so we can always clear it cleanly
    private Vector2Int? _lastHighlightOrigin;
    private bool        _lastHighlightWasSet;

    public DragDropPresenter(
      IBagPresenter         bagPresenter,
      IBottomSlotsPresenter slotsPresenter,
      IGridDragDropService  dragDropService,
      IDragIconViewModel    dragIconViewModel,
      IAssetLoader          assetLoader)
    {
      _bagPresenter      = bagPresenter;
      _slotsPresenter    = slotsPresenter;
      _dragDropService   = dragDropService;
      _dragIconViewModel = dragIconViewModel;
      _assetLoader       = assetLoader;
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

    public void UpdateDragPosition(Vector2 screenPosition) =>
      _dragIconViewModel.UpdatePosition(screenPosition);

    #endregion

    #region End — void drop

    public void HandleEndDrag()
    {
      _dragIconViewModel.Hide();
      ClearLastHighlight();

      if (!_dragDropService.IsDragging) return;

      if (_dragDropService.Source == DragSource.Bag)
      {
        var item = _dragDropService.DraggedItem;
        if (_slotsPresenter.TryPlaceInFirstFreeSlot(item, out _))
          _dragDropService.EndDrag();
        else
          _dragDropService.CancelDrag();
      }
      else
      {
        _dragDropService.CancelDrag();
      }
    }

    #endregion

    #region Drop on cell

    public void HandleDropOnCell(Vector2Int cellCoord)
    {
      _dragIconViewModel.Hide();
      ClearLastHighlight();

      if (!_dragDropService.IsDragging) return;

      var dragged      = _dragDropService.DraggedItem;
      var targetOrigin = cellCoord - _dragDropService.DragOffset;

      if (_bagPresenter.CanMerge(dragged, cellCoord, out var targetItem))
      {
        _bagPresenter.Merge(dragged, targetItem);
        _dragDropService.EndDrag();
        return;
      }

      dragged.SetOrigin(targetOrigin);
      if (_bagPresenter.TryPlace(dragged))
      {
        _dragDropService.EndDrag();
        return;
      }

      _dragDropService.CancelDrag();
    }

    #endregion

    #region Drop on slot

    public void HandleDropOnSlot(int slotIndex)
    {
      _dragIconViewModel.Hide();

      if (!_dragDropService.IsDragging) return;

      var dragged     = _dragDropService.DraggedItem;
      var currentItem = _slotsPresenter.GetSlot(slotIndex);

      if (currentItem == null)
      {
        _slotsPresenter.TryPlace(dragged, slotIndex);
        _dragDropService.EndDrag();
        return;
      }

      _slotsPresenter.TryRemove(slotIndex, out var displaced);

      int  sourceSlot   = _dragDropService.SourceSlotIndex;
      bool swapPossible = _dragDropService.Source == DragSource.BottomSlot
                          && sourceSlot >= 0
                          && _slotsPresenter.TryPlace(displaced, sourceSlot);

      if (swapPossible)
      {
        _slotsPresenter.TryPlace(dragged, slotIndex);
        _dragDropService.EndDrag();
      }
      else if (_slotsPresenter.TryPlaceInFirstFreeSlot(displaced, out _))
      {
        _slotsPresenter.TryPlace(dragged, slotIndex);
        _dragDropService.EndDrag();
      }
      else
      {
        _slotsPresenter.TryPlace(displaced, slotIndex);
        _dragDropService.CancelDrag();
      }
    }

    #endregion

    #region Highlight

    public void HandlePointerEnterCell(Vector2Int cellCoord)
    {
      if (!_dragDropService.IsDragging) return;

      var dragged      = _dragDropService.DraggedItem;
      var targetOrigin = cellCoord - _dragDropService.DragOffset;

      // Always clear the previous highlight first to avoid ghost highlights
      // when the pointer moves quickly across cells
      ClearLastHighlight();

      HighlightState state;

      if (_bagPresenter.CanMerge(dragged, cellCoord, out _))
        state = HighlightState.Merge;
      else if (_bagPresenter.CanPlace(dragged.Config, targetOrigin, dragged))
        state = HighlightState.Valid;
      else
        state = HighlightState.Invalid;

      _bagPresenter.RequestHighlight(dragged.Config, targetOrigin, state);

      // Remember what we highlighted so we can clear it on the next Enter or drag end
      _lastHighlightOrigin = targetOrigin;
      _lastHighlightWasSet  = true;
    }

    public void HandlePointerExitCell(Vector2Int cellCoord)
    {
      if (!_dragDropService.IsDragging) return;

      // Clear by explicit exit coord as a fallback; ClearLastHighlight handles
      // the normal case via the tracked origin
      ClearLastHighlight();
    }

    /// <summary>Clears the highlight shape we last painted, if any.</summary>
    private void ClearLastHighlight()
    {
      if (!_lastHighlightWasSet || !_dragDropService.IsDragging) return;

      var dragged = _dragDropService.DraggedItem;
      if (dragged == null) return;

      _bagPresenter.RequestHighlight(
        dragged.Config,
        _lastHighlightOrigin!.Value,
        HighlightState.None);

      _lastHighlightWasSet  = false;
      _lastHighlightOrigin  = null;
    }

    #endregion
  }
}
