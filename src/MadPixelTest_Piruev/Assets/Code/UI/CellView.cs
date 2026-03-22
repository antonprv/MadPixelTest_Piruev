// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System;

using Code.Core;
using Code.Data.StaticData;
using Code.Infrastructure.AssetManagement;
using Code.Services.Interfaces;
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
  /// Single grid cell.
  ///
  /// Visual:
  ///   _background  — cell background (changes color: empty / occupied)
  ///   _iconImage   — item icon, displayed only on item's origin cell
  ///   _highlight   — semi-transparent overlay: green/red/gold on drag-over
  ///
  /// Drag & Drop:
  ///   OnBeginDrag → removes item from grid, passes to DragDropService, shows DragIconView
  ///   OnDrag      → moves DragIconView
  ///   OnEndDrag   → if DragDropService still active (drop in void) → CancelDrag
  ///   OnDrop      → tries merge / place; on failure → CancelDrag
  ///   OnPointerEnter/Exit → controls highlight via callback from BagView
  /// </summary>
  public class CellView : ZenjexBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IDropHandler,
    IPointerEnterHandler, IPointerExitHandler
  {
    [Header("Visual")]
    [SerializeField] private Image _background;
    [SerializeField] private Image _iconImage;
    [SerializeField] private Image _highlight;

    [Header("Colors")]
    [SerializeField] private Color _emptyColor = new(0.15f, 0.15f, 0.15f, 0.6f);
    [SerializeField] private Color _validColor = new(0.0f, 0.9f, 0.2f, 0.45f);
    [SerializeField] private Color _invalidColor = new(0.9f, 0.1f, 0.0f, 0.45f);
    [SerializeField] private Color _mergeColor = new(1.0f, 0.75f, 0.0f, 0.55f);

    [Header("Animation")]
    [SerializeField] private float _placeDuration = 0.12f;

    // ─── Injected ─────────────────────────────────────────────────────────────
    [Zenjex] private IGridInventoryService _inventoryService;
    [Zenjex] private IBottomSlotsService _slotsService;
    [Zenjex] private IGridDragDropService _dragDropService;
    [Zenjex] private DragIconView _dragIconView;
    [Zenjex] private IAssetLoader _assetLoader;

    // ─── State ────────────────────────────────────────────────────────────────
    private Vector2Int _cellCoord;
    private bool _isActive;

    // Callback to BagView for highlighting multiple cells at once
    private Action<ItemConfig, Vector2Int, HighlightState> _onHighlightRequest;

    // ─── Init ─────────────────────────────────────────────────────────────────

    public void Initialize(
      Vector2Int coord,
      bool isActive,
      Action<ItemConfig, Vector2Int, HighlightState> onHighlightRequest)
    {
      _cellCoord = coord;
      _isActive = isActive;
      _onHighlightRequest = onHighlightRequest;

      if (!isActive)
      {
        gameObject.SetActive(false);
        return;
      }

      SetHighlight(HighlightState.None);
      RefreshView();
    }

    // ─── View refresh ─────────────────────────────────────────────────────────

    public void RefreshView() => RefreshViewAsync().Forget();

    private async UniTaskVoid RefreshViewAsync()
    {
      var item = _inventoryService.GetItemAt(_cellCoord);
      bool isEmpty = item == null;

      _background.color = isEmpty ? _emptyColor : item.Config.ItemColor;

      if (_iconImage == null) return;

      bool isOrigin = !isEmpty && item.Origin == _cellCoord;
      _iconImage.enabled = isOrigin;

      if (isOrigin && item.Config.Icon != null)
        _iconImage.sprite = await _assetLoader.LoadAsync<Sprite>(item.Config.Icon);
    }

    public void SetHighlight(HighlightState state)
    {
      if (_highlight == null) return;

      switch (state)
      {
        case HighlightState.None:
          _highlight.enabled = false;
          break;
        case HighlightState.Valid:
          _highlight.enabled = true;
          _highlight.color = _validColor;
          break;
        case HighlightState.Invalid:
          _highlight.enabled = true;
          _highlight.color = _invalidColor;
          break;
        case HighlightState.Merge:
          _highlight.enabled = true;
          _highlight.color = _mergeColor;
          break;
      }
    }

    // ─── IBeginDragHandler ────────────────────────────────────────────────────

    public void OnBeginDrag(PointerEventData eventData)
    {
      var item = _inventoryService.GetItemAt(_cellCoord);
      if (item == null) return;

      var dragOffset = _cellCoord - item.Origin;
      _inventoryService.TryRemove(item);
      _dragDropService.StartDrag(item, DragSource.Bag, dragOffset);

      // Show icon immediately from cache — preloader has already warmed up all sprites by this point
      ShowDragIconAsync(item, eventData.position).Forget();
      RefreshView();
    }

    private async UniTaskVoid ShowDragIconAsync(InventoryItem item, Vector2 position)
    {
      var sprite = item.Config.Icon != null
        ? await _assetLoader.LoadAsync<Sprite>(item.Config.Icon)
        : null;

      // Check that drag is still active (user might have released)
      if (_dragDropService.IsDragging && sprite != null)
        _dragIconView.Show(sprite, position);
    }

    // ─── IDragHandler ─────────────────────────────────────────────────────────

    public void OnDrag(PointerEventData eventData)
    {
      _dragIconView.UpdatePosition(eventData.position);
    }

    // ─── IEndDragHandler ─────────────────────────────────────────────────────

    public void OnEndDrag(PointerEventData eventData)
    {
      _dragIconView.Hide();

      // If IsDragging is still true — OnDrop didn't trigger (drop in void / outside UI)
      if (!_dragDropService.IsDragging) return;

      var item = _dragDropService.DraggedItem;

      // Try to return to bottom slots
      if (_slotsService.TryPlaceInFirstFreeSlot(item, out _))
      {
        _dragDropService.EndDrag();
      }
      else
      {
        // Slots are full — return back to bag
        _dragDropService.CancelDrag();
      }

      RefreshView();
    }

    // ─── IDropHandler ─────────────────────────────────────────────────────────

    public void OnDrop(PointerEventData eventData)
    {
      if (!_dragDropService.IsDragging) return;

      var dragged = _dragDropService.DraggedItem;

      // origin = cell under cursor minus grab offset
      var targetOrigin = _cellCoord - _dragDropService.DragOffset;

      // 1. Check merge (by cell under cursor)
      if (_inventoryService.CanMerge(dragged, _cellCoord, out var targetItem))
      {
        var merged = _inventoryService.Merge(dragged, targetItem);
        _dragDropService.EndDrag();
        PlayPlaceAnimation(merged.Origin);
        RefreshView();
        return;
      }

      // 2. Try to place with calculated origin
      dragged.SetOrigin(targetOrigin);
      if (_inventoryService.TryPlace(dragged))
      {
        _dragDropService.EndDrag();
        PlayPlaceAnimation(targetOrigin);
        RefreshView();
        return;
      }

      // 3. Failed → CancelDrag (return to original position)
      _dragDropService.CancelDrag();
      RefreshView();
    }

    // ─── IPointerEnterHandler / IPointerExitHandler ───────────────────────────

    public void OnPointerEnter(PointerEventData eventData)
    {
      if (!_dragDropService.IsDragging) return;

      var dragged = _dragDropService.DraggedItem;
      var targetOrigin = _cellCoord - _dragDropService.DragOffset;

      HighlightState state;

      if (_inventoryService.CanMerge(dragged, _cellCoord, out _))
        state = HighlightState.Merge;
      else if (_inventoryService.CanPlace(dragged.Config, targetOrigin, dragged))
        state = HighlightState.Valid;
      else
        state = HighlightState.Invalid;

      // Highlight item shape starting from targetOrigin
      _onHighlightRequest?.Invoke(dragged.Config, targetOrigin, state);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
      if (!_dragDropService.IsDragging) return;
      var targetOrigin = _cellCoord - _dragDropService.DragOffset;
      _onHighlightRequest?.Invoke(_dragDropService.DraggedItem.Config, targetOrigin, HighlightState.None);
    }

    // ─── Animation ────────────────────────────────────────────────────────────

    private void PlayPlaceAnimation(Vector2Int origin)
    {
      // Scale "pop" on place — run on origin cell's icon
      if (_cellCoord != origin) return;

      transform.localScale = Vector3.one * 0.8f;
      LeanTween
        .scale(gameObject, Vector3.one, _placeDuration)
        .setEaseOutBack();
    }
  }
}
