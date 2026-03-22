// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Code.UI.Types;
using Code.ViewModel.Cell;

using R3;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using Zenjex.Extensions.Injector;

namespace Code.View
{
  /// <summary>
  /// MVVM View — single grid cell.
  ///
  /// Layer structure (bottom → top):
  ///   _background   — neutral color, never changes
  ///   _itemOverlay  — item tint color (cell_bg sprite, alpha=0 when empty)
  ///   _highlight    — drag-preview overlay
  ///
  /// Icon (sprite) is NOT here anymore. BagView creates a separate full-size
  /// Image per item that spans its entire bounding box.
  /// </summary>
  public class CellView : ZenjexBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IDropHandler,
    IPointerEnterHandler, IPointerExitHandler
  {
    [Header("Visual")]
    [SerializeField] private Image _background;
    [SerializeField] private Image _itemOverlay;
    [SerializeField] private Image _highlight;

    [Header("Animation")]
    [SerializeField] private float _placeDuration = 0.12f;

    [Header("Highlight Colors")]
    [SerializeField] private Color _validColor   = new(0.0f, 0.9f, 0.2f, 0.55f);
    [SerializeField] private Color _invalidColor = new(0.9f, 0.1f, 0.0f, 0.55f);
    [SerializeField] private Color _mergeColor   = new(1.0f, 0.75f, 0.0f, 0.65f);

    private ICellViewModel      _viewModel;
    private CompositeDisposable _disposables;

    public void SetViewModel(ICellViewModel viewModel, Vector2Int coord, bool isActive)
    {
      _viewModel = viewModel;

      if (!isActive)
      {
        gameObject.SetActive(false);
        return;
      }

      _disposables?.Dispose();
      _disposables = new CompositeDisposable();

      Bind();
    }

    private void OnDestroy() => _disposables?.Dispose();

    private void Bind()
    {
      _viewModel.BackgroundColor
        .Subscribe(c => _background.color = c)
        .AddTo(_disposables);

      _viewModel.ItemOverlayColor
        .Subscribe(c => { if (_itemOverlay != null) _itemOverlay.color = c; })
        .AddTo(_disposables);

      _viewModel.Highlight
        .Subscribe(ApplyHighlight)
        .AddTo(_disposables);
    }

    private void ApplyHighlight(HighlightState state)
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

    #region Input

    public void OnBeginDrag(PointerEventData e)  => _viewModel?.OnBeginDrag(e.position);
    public void OnDrag(PointerEventData e)        => _viewModel?.OnDrag(e.position);
    public void OnEndDrag(PointerEventData e)     { _viewModel?.OnEndDrag(); PlayPlaceAnimation(); }
    public void OnDrop(PointerEventData e)        { _viewModel?.OnDrop();    PlayPlaceAnimation(); }
    public void OnPointerEnter(PointerEventData e) => _viewModel?.OnPointerEnter();
    public void OnPointerExit(PointerEventData e)  => _viewModel?.OnPointerExit();

    #endregion

    private void PlayPlaceAnimation()
    {
      transform.localScale = Vector3.one * 0.85f;
      LeanTween.scale(gameObject, Vector3.one, _placeDuration).setEaseOutBack();
    }
  }
}
