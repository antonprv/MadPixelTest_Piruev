using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using BagFight.Core;
using BagFight.Infrastructure.AssetManagement;
using BagFight.Services.Interfaces;
using BagFight.UI.Types;
using Zenjex.Extensions.Attribute;
using Zenjex.Extensions.Injector;

namespace BagFight.UI
{
  /// <summary>
  /// Один слот снизу.
  ///
  /// Отличается от CellView:
  ///   - Хранит целый предмет (любой формы), а не одну клетку
  ///   - При OnDrop из сумки возвращает предмет в слот
  ///   - При OnBeginDrag снимает предмет из слота и отдаёт в DragDropService
  ///
  /// Краевой случай — слот занят, а сюда пытаются дропнуть:
  ///   Производим своп: старый предмет уходит в первый свободный слот,
  ///   новый встаёт сюда. Если своп невозможен — CancelDrag.
  /// </summary>
  public class BottomSlotView : ZenjexBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IDropHandler
  {
    [Header("Visual")]
    [SerializeField] private Image _background;
    [SerializeField] private Image _iconImage;

    [Header("Colors")]
    [SerializeField] private Color _emptyColor    = new(0.12f, 0.12f, 0.12f, 0.5f);
    [SerializeField] private Color _occupiedColor = new(0.25f, 0.25f, 0.25f, 0.8f);

    [Header("Animation")]
    [SerializeField] private float _bounceDuration = 0.15f;

    // ─── Injected ─────────────────────────────────────────────────────────────
    [Zenjex] private IBottomSlotsService  _slotsService;
    [Zenjex] private IGridDragDropService _dragDropService;
    [Zenjex] private DragIconView         _dragIconView;
    [Zenjex] private IAssetLoader         _assetLoader;

    // ─── State ────────────────────────────────────────────────────────────────
    private int _slotIndex;

    // ─── Init ─────────────────────────────────────────────────────────────────

    public void Initialize(int slotIndex)
    {
      _slotIndex = slotIndex;
      RefreshView();
    }

    // ─── View ─────────────────────────────────────────────────────────────────

    public void RefreshView() => RefreshViewAsync().Forget();

    private async UniTaskVoid RefreshViewAsync()
    {
      var item  = _slotsService.GetSlot(_slotIndex);
      bool empty = item == null;

      _background.color = empty ? _emptyColor : _occupiedColor;

      if (_iconImage == null) return;
      _iconImage.enabled = !empty;

      if (!empty && item.Config.Icon != null)
        _iconImage.sprite = await _assetLoader.LoadAsync<Sprite>(item.Config.Icon);
    }

    // ─── IBeginDragHandler ────────────────────────────────────────────────────

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

    // ─── IDragHandler ─────────────────────────────────────────────────────────

    public void OnDrag(PointerEventData eventData)
    {
      _dragIconView.UpdatePosition(eventData.position);
    }

    // ─── IEndDragHandler ─────────────────────────────────────────────────────

    public void OnEndDrag(PointerEventData eventData)
    {
      _dragIconView.Hide();

      // Если IsDragging всё ещё true → дроп в пустоту → CancelDrag
      if (_dragDropService.IsDragging)
      {
        _dragDropService.CancelDrag();
        RefreshView();
      }
    }

    // ─── IDropHandler ─────────────────────────────────────────────────────────

    public void OnDrop(PointerEventData eventData)
    {
      if (!_dragDropService.IsDragging) return;

      var dragged    = _dragDropService.DraggedItem;
      var currentItem = _slotsService.GetSlot(_slotIndex);

      if (currentItem == null)
      {
        // Слот пуст — просто кладём
        _slotsService.TryPlace(dragged, _slotIndex);
        _dragDropService.EndDrag();
      }
      else
      {
        // Слот занят — пробуем своп:
        // старый предмет уходит в первый свободный слот (не этот)
        _slotsService.TryRemove(_slotIndex, out var displaced);

        if (_slotsService.TryPlaceInFirstFreeSlot(displaced, out _))
        {
          // Своп удался
          _slotsService.TryPlace(dragged, _slotIndex);
          _dragDropService.EndDrag();
        }
        else
        {
          // Свободных слотов нет — возвращаем displaced обратно, отменяем drag
          _slotsService.TryPlace(displaced, _slotIndex);
          _dragDropService.CancelDrag();
        }
      }

      RefreshView();
      PlayBounce();
    }

    // ─── Animation ────────────────────────────────────────────────────────────

    private void PlayBounce()
    {
      transform.localScale = Vector3.one * 0.85f;
      LeanTween
        .scale(gameObject, Vector3.one, _bounceDuration)
        .setEaseOutBack();
    }
  }
}
