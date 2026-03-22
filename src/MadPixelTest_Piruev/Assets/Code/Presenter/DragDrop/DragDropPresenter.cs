// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Infrastructure.AssetManagement;
using Code.Infrastructure.Services;
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
  /// Changes:
  ///   - Show() passes itemBounds → DragIconView sizes icon at ½ grid footprint
  ///   - Cancel/return paths call FlyTo() → icon animates back to slot
  ///   - SlotPositionProviderLocator used for screen positions (late-bound by UIFactory)
  /// </summary>
  public class DragDropPresenter : IDragDropPresenter
  {
    private readonly IBagPresenter         _bagPresenter;
    private readonly IBottomSlotsPresenter  _slotsPresenter;
    private readonly IGridDragDropService   _dragDropService;
    private readonly IDragIconViewModel     _dragIconViewModel;
    private readonly IAssetLoader           _assetLoader;

    private Vector2Int? _lastHighlightOrigin;
    private bool        _lastHighlightWasSet;

    public DragDropPresenter(
      IBagPresenter         bagPresenter,
      IBottomSlotsPresenter  slotsPresenter,
      IGridDragDropService   dragDropService,
      IDragIconViewModel     dragIconViewModel,
      IAssetLoader           assetLoader)
    {
      _bagPresenter      = bagPresenter;
      _slotsPresenter    = slotsPresenter;
      _dragDropService   = dragDropService;
      _dragIconViewModel = dragIconViewModel;
      _assetLoader       = assetLoader;
    }

    #region Start drag

    public void StartDragFromBag(Vector2Int cellCoord, Vector2 screenPosition)
    {
      var item = _bagPresenter.GetItemAt(cellCoord);
      if (item == null) return;

      var dragOffset = cellCoord - item.Origin;
      _bagPresenter.TryRemove(item);
      _dragDropService.StartDrag(item, DragSource.Bag, dragOffset);

      ShowDragIconAsync(item, screenPosition).Forget();
    }

    public void StartDragFromSlot(int slotIndex, Vector2 screenPosition)
    {
      if (!_slotsPresenter.TryRemove(slotIndex, out var item)) return;

      _dragDropService.StartDrag(item, DragSource.BottomSlot, Vector2Int.zero, slotIndex);

      ShowDragIconAsync(item, screenPosition).Forget();
    }

    private async UniTaskVoid ShowDragIconAsync(
      Code.Model.Core.InventoryItem item,
      Vector2 screenPosition)
    {
      var sprite = item.Config.Icon != null
        ? await _assetLoader.LoadAsync<Sprite>(item.Config.Icon)
        : null;

      if (!_dragDropService.IsDragging) return;
      if (sprite == null) return;

      _dragIconViewModel.Show(sprite, screenPosition, item.Config.GetBoundsSize());
    }

    #endregion

    #region Move

    public void UpdateDragPosition(Vector2 screenPosition) =>
      _dragIconViewModel.UpdatePosition(screenPosition);

    #endregion

    #region End — void drop

    public void HandleEndDrag()
    {
      ClearLastHighlight();

      if (!_dragDropService.IsDragging)
      {
        _dragIconViewModel.Hide();
        return;
      }

      if (_dragDropService.Source == DragSource.Bag)
      {
        var item = _dragDropService.DraggedItem;
        if (_slotsPresenter.TryPlaceInFirstFreeSlot(item, out int placedSlot))
        {
          _dragDropService.EndDrag();
          FlyIconToSlot(placedSlot);
        }
        else
        {
          // No free slot — return item to bag, no fly animation
          _dragIconViewModel.Hide();
          _dragDropService.CancelDrag();
        }
      }
      else // BottomSlot source — return to source slot
      {
        int sourceSlot = _dragDropService.SourceSlotIndex;
        _dragDropService.CancelDrag();
        FlyIconToSlot(sourceSlot);
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
      ClearLastHighlight();

      if (!_dragDropService.IsDragging)
      {
        _dragIconViewModel.Hide();
        return;
      }

      var dragged     = _dragDropService.DraggedItem;
      var currentItem = _slotsPresenter.GetSlot(slotIndex);

      if (currentItem == null)
      {
        _slotsPresenter.TryPlace(dragged, slotIndex);
        _dragDropService.EndDrag();
        FlyIconToSlot(slotIndex);
        return;
      }

      // Swap
      _slotsPresenter.TryRemove(slotIndex, out var displaced);
      int  sourceSlot   = _dragDropService.SourceSlotIndex;
      bool swapPossible = _dragDropService.Source == DragSource.BottomSlot
                          && sourceSlot >= 0
                          && _slotsPresenter.TryPlace(displaced, sourceSlot);

      if (swapPossible)
      {
        _slotsPresenter.TryPlace(dragged, slotIndex);
        _dragDropService.EndDrag();
        FlyIconToSlot(slotIndex);
      }
      else if (_slotsPresenter.TryPlaceInFirstFreeSlot(displaced, out _))
      {
        _slotsPresenter.TryPlace(dragged, slotIndex);
        _dragDropService.EndDrag();
        FlyIconToSlot(slotIndex);
      }
      else
      {
        _slotsPresenter.TryPlace(displaced, slotIndex);
        _dragDropService.CancelDrag();
        _dragIconViewModel.Hide();
      }
    }

    #endregion

    #region Highlight

    public void HandlePointerEnterCell(Vector2Int cellCoord)
    {
      if (!_dragDropService.IsDragging) return;

      var dragged      = _dragDropService.DraggedItem;
      var targetOrigin = cellCoord - _dragDropService.DragOffset;

      ClearLastHighlight();

      HighlightState state;
      if (_bagPresenter.CanMerge(dragged, cellCoord, out _))
        state = HighlightState.Merge;
      else if (_bagPresenter.CanPlace(dragged.Config, targetOrigin, dragged))
        state = HighlightState.Valid;
      else
        state = HighlightState.Invalid;

      _bagPresenter.RequestHighlight(dragged.Config, targetOrigin, state);
      _lastHighlightOrigin = targetOrigin;
      _lastHighlightWasSet  = true;
    }

    public void HandlePointerExitCell(Vector2Int cellCoord) =>
      ClearLastHighlight();

    private void ClearLastHighlight()
    {
      if (!_lastHighlightWasSet || !_dragDropService.IsDragging) return;
      var dragged = _dragDropService.DraggedItem;
      if (dragged == null) return;

      _bagPresenter.RequestHighlight(
        dragged.Config, _lastHighlightOrigin!.Value, HighlightState.None);

      _lastHighlightWasSet  = false;
      _lastHighlightOrigin  = null;
    }

    #endregion

    #region Helpers

    private void FlyIconToSlot(int slotIndex)
    {
      var provider  = SlotPositionProviderLocator.Instance;
      var targetPos = provider?.GetSlotScreenPosition(slotIndex) ?? Vector2.zero;

      if (targetPos == Vector2.zero)
        _dragIconViewModel.Hide();
      else
        _dragIconViewModel.FlyTo(targetPos);
    }

    #endregion
  }
}
