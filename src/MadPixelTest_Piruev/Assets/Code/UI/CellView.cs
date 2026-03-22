using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using BagFight.Core;
using BagFight.Services.Interfaces;
using BagFight.UI.Types;
using Zenjex.Extensions.Attribute;
using Zenjex.Extensions.Injector;

namespace BagFight.UI
{
  /// <summary>
  /// Одна ячейка грида.
  ///
  /// Визуал:
  ///   _background  — фон ячейки (меняет цвет: пусто / занято)
  ///   _iconImage   — иконка предмета, отображается только на origin-клетке предмета
  ///   _highlight   — полупрозрачный оверлей: зелёный/красный/золотой при drag-over
  ///
  /// Drag & Drop:
  ///   OnBeginDrag → снимает предмет с грида, отдаёт в DragDropService, показывает DragIconView
  ///   OnDrag      → двигает DragIconView
  ///   OnEndDrag   → если DragDropService ещё активен (дроп в пустоту) → CancelDrag
  ///   OnDrop      → пробует merge / place; при неудаче → CancelDrag
  ///   OnPointerEnter/Exit → управляет подсветкой через коллбэк из BagView
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
    [SerializeField] private Color _emptyColor    = new(0.15f, 0.15f, 0.15f, 0.6f);
    [SerializeField] private Color _validColor    = new(0.0f,  0.9f,  0.2f,  0.45f);
    [SerializeField] private Color _invalidColor  = new(0.9f,  0.1f,  0.0f,  0.45f);
    [SerializeField] private Color _mergeColor    = new(1.0f,  0.75f, 0.0f,  0.55f);

    [Header("Animation")]
    [SerializeField] private float _placeDuration = 0.12f;

    // ─── Injected ─────────────────────────────────────────────────────────────
    [Zenjex] private IGridInventoryService _inventoryService;
    [Zenjex] private IBottomSlotsService   _slotsService;
    [Zenjex] private IGridDragDropService  _dragDropService;
    [Zenjex] private DragIconView          _dragIconView;

    // ─── State ────────────────────────────────────────────────────────────────
    private Vector2Int _cellCoord;
    private bool       _isActive;

    // Коллбэк к BagView для подсветки нескольких клеток сразу
    private Action<ItemConfig, Vector2Int, HighlightState> _onHighlightRequest;

    // ─── Init ─────────────────────────────────────────────────────────────────

    public void Initialize(
      Vector2Int coord,
      bool isActive,
      Action<ItemConfig, Vector2Int, HighlightState> onHighlightRequest)
    {
      _cellCoord          = coord;
      _isActive           = isActive;
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

    public void RefreshView()
    {
      var item     = _inventoryService.GetItemAt(_cellCoord);
      bool isEmpty = item == null;

      // Фон
      _background.color = isEmpty ? _emptyColor : item.Config.ItemColor;

      // Иконка — только на origin-клетке предмета
      if (_iconImage != null)
      {
        bool isOrigin = !isEmpty && item.Origin == _cellCoord;
        _iconImage.enabled = isOrigin;
        if (isOrigin)
          _iconImage.sprite = item.Config.Icon;
      }
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
          _highlight.color   = _validColor;
          break;
        case HighlightState.Invalid:
          _highlight.enabled = true;
          _highlight.color   = _invalidColor;
          break;
        case HighlightState.Merge:
          _highlight.enabled = true;
          _highlight.color   = _mergeColor;
          break;
      }
    }

    // ─── IBeginDragHandler ────────────────────────────────────────────────────

    public void OnBeginDrag(PointerEventData eventData)
    {
      var item = _inventoryService.GetItemAt(_cellCoord);
      if (item == null) return;

      // Смещение: от захваченной ячейки до origin предмета.
      // Пример: origin=(1,0), захвачена ячейка (1,2) → offset=(0,2)
      var dragOffset = _cellCoord - item.Origin;

      // Убираем предмет с грида (ячейки освобождаются)
      _inventoryService.TryRemove(item);

      // Регистрируем драг с offset'ом
      _dragDropService.StartDrag(item, DragSource.Bag, dragOffset);

      // Показываем плавающую иконку
      _dragIconView.Show(item.Config.Icon, eventData.position);

      RefreshView();
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

      // Если IsDragging всё ещё true — OnDrop не сработал (дроп в пустоту / вне UI)
      if (!_dragDropService.IsDragging) return;

      var item = _dragDropService.DraggedItem;

      // Пробуем вернуть в слоты снизу
      if (_slotsService.TryPlaceInFirstFreeSlot(item, out _))
      {
        _dragDropService.EndDrag();
      }
      else
      {
        // Слоты полны — возвращаем обратно в сумку
        _dragDropService.CancelDrag();
      }

      RefreshView();
    }

    // ─── IDropHandler ─────────────────────────────────────────────────────────

    public void OnDrop(PointerEventData eventData)
    {
      if (!_dragDropService.IsDragging) return;

      var dragged = _dragDropService.DraggedItem;

      // origin = ячейка под курсором минус смещение захвата
      var targetOrigin = _cellCoord - _dragDropService.DragOffset;

      // 1. Проверяем мерж (по ячейке под курсором)
      if (_inventoryService.CanMerge(dragged, _cellCoord, out var targetItem))
      {
        var merged = _inventoryService.Merge(dragged, targetItem);
        _dragDropService.EndDrag();
        PlayPlaceAnimation(merged.Origin);
        RefreshView();
        return;
      }

      // 2. Пробуем разместить с вычисленным origin
      dragged.SetOrigin(targetOrigin);
      if (_inventoryService.TryPlace(dragged))
      {
        _dragDropService.EndDrag();
        PlayPlaceAnimation(targetOrigin);
        RefreshView();
        return;
      }

      // 3. Не получилось → CancelDrag (вернуть на исходное место)
      _dragDropService.CancelDrag();
      RefreshView();
    }

    // ─── IPointerEnterHandler / IPointerExitHandler ───────────────────────────

    public void OnPointerEnter(PointerEventData eventData)
    {
      if (!_dragDropService.IsDragging) return;

      var dragged     = _dragDropService.DraggedItem;
      var targetOrigin = _cellCoord - _dragDropService.DragOffset;

      HighlightState state;

      if (_inventoryService.CanMerge(dragged, _cellCoord, out _))
        state = HighlightState.Merge;
      else if (_inventoryService.CanPlace(dragged.Config, targetOrigin, dragged))
        state = HighlightState.Valid;
      else
        state = HighlightState.Invalid;

      // Подсвечиваем форму предмета начиная с targetOrigin
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
      // Масштабный «поп» при размещении — запускаем на иконке origin-клетки
      if (_cellCoord != origin) return;

      transform.localScale = Vector3.one * 0.8f;
      LeanTween
        .scale(gameObject, Vector3.one, _placeDuration)
        .setEaseOutBack();
    }
  }
}
