// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Code.UI.Services.BottomSlots.Interfaces;
using Code.UI.Services.DragDrop.Interfaces;

using Code.Core;
using Code.Infrastructure.AssetManagement;
using Code.UI.Types;

using Cysharp.Threading.Tasks;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using Zenjex.Extensions.Attribute;
using Zenjex.Extensions.Injector;

namespace Code.UI
{
  /// <summary>
  /// Single bottom slot.
  ///
  /// Differs from CellView:
  ///   - Stores entire item (any shape), not a single cell
  ///   - OnDrop from bag returns item to slot
  ///   - OnBeginDrag removes item from slot and passes to DragDropService
  ///
  /// Edge case — slot is occupied and something is dropped here:
  ///   Perform swap: old item goes to first free slot,
  ///   new one takes its place. If swap is impossible — CancelDrag.
  /// </summary>
  public class BottomSlotView : ZenjexBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IDropHandler
  {
    [Header("Visual")]
    [SerializeField] private Image _background;
    [SerializeField] private Image _iconImage;

    [Header("Colors")]
    [SerializeField] private Color _emptyColor = new(0.12f, 0.12f, 0.12f, 0.5f);
    [SerializeField] private Color _occupiedColor = new(0.25f, 0.25f, 0.25f, 0.8f);

    [Header("Animation")]
    [SerializeField] private float _bounceDuration = 0.15f;

    #region Injected
    [Zenjex] private IBottomSlotsService _slotsService;
    [Zenjex] private IGridDragDropService _dragDropService;
    [Zenjex] private DragIconView _dragIconView;
    [Zenjex] private IAssetLoader _assetLoader;
    #endregion

    #region State
    private int _slotIndex;
    #endregion

    #region Init

    public void Initialize(int slotIndex)
    {
      _slotIndex = slotIndex;
      RefreshView();
    }

    #endregion

    #region View

    public void RefreshView() => RefreshViewAsync().Forget();

    private async UniTaskVoid RefreshViewAsync()
    {
      var item = _slotsService.GetSlot(_slotIndex);
      bool empty = item == null;

      _background.color = empty ? _emptyColor : _occupiedColor;

      if (_iconImage == null) return;
      _iconImage.enabled = !empty;

      if (!empty && item.Config.Icon != null)
        _iconImage.sprite = await _assetLoader.LoadAsync<Sprite>(item.Config.Icon);
    }

    #endregion

    #region IBeginDragHandler

    public void OnBeginDrag(PointerEventData eventData)
    {
      if (!_slotsService.TryRemove(_slotIndex, out var item)) return;

      _dragDropService.StartDrag(item, DragSource.BottomSlot, Vector2Int.zero, _slotIndex);
      ShowDragIconAsync(item, eventData.position).Forget();
      RefreshView();
    }

    private async UniTaskVoid ShowDragIconAsync(InventoryItem item, Vector2 position)
    {
      var sprite = item.Config.Icon != null
        ? await _assetLoader.LoadAsync<Sprite>(item.Config.Icon)
        : null;

      if (_dragDropService.IsDragging && sprite != null)
        _dragIconView.Show(sprite, position);
    }

    #endregion

    #region IDragHandler

    public void OnDrag(PointerEventData eventData)
    {
      _dragIconView.UpdatePosition(eventData.position);
    }

    #endregion

    #region IEndDragHandler

    public void OnEndDrag(PointerEventData eventData)
    {
      _dragIconView.Hide();

      // If IsDragging is still true → drop in void → CancelDrag
      if (_dragDropService.IsDragging)
      {
        _dragDropService.CancelDrag();
        RefreshView();
      }
    }

    #endregion

    #region IDropHandler

    public void OnDrop(PointerEventData eventData)
    {
      if (!_dragDropService.IsDragging) return;

      var dragged = _dragDropService.DraggedItem;
      var currentItem = _slotsService.GetSlot(_slotIndex);

      if (currentItem == null)
      {
        // Slot is empty — just place it
        _slotsService.TryPlace(dragged, _slotIndex);
        _dragDropService.EndDrag();
      }
      else
      {
        // Slot is occupied — try swap:
        // old item goes to first free slot (not this one)
        _slotsService.TryRemove(_slotIndex, out var displaced);

        if (_slotsService.TryPlaceInFirstFreeSlot(displaced, out _))
        {
          // Swap succeeded
          _slotsService.TryPlace(dragged, _slotIndex);
          _dragDropService.EndDrag();
        }
        else
        {
          // No free slots — return displaced back, cancel drag
          _slotsService.TryPlace(displaced, _slotIndex);
          _dragDropService.CancelDrag();
        }
      }

      RefreshView();
      PlayBounce();
    }

    #endregion

    #region Animation

    private void PlayBounce()
    {
      transform.localScale = Vector3.one * 0.85f;
      LeanTween
        .scale(gameObject, Vector3.one, _bounceDuration)
        .setEaseOutBack();
    }

    #endregion
  }
}
