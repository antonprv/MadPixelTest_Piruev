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
  /// What it does:
  ///   - Binds ReactiveProperties from ICellViewModel to Unity UI components
  ///   - Forwards raw Unity input events to ViewModel commands
  ///
  /// What it does NOT do:
  ///   - No service references
  ///   - No business logic
  ///   - No asset loading
  ///   - No drag state management
  ///
  /// ViewModel is assigned by BagView via SetViewModel().
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

    [Header("Animation")]
    [SerializeField] private float _placeDuration = 0.12f;

    [Header("Highlight Colors")]
    [SerializeField] private Color _validColor = new(0.0f, 0.9f, 0.2f, 0.45f);
    [SerializeField] private Color _invalidColor = new(0.9f, 0.1f, 0.0f, 0.45f);
    [SerializeField] private Color _mergeColor = new(1.0f, 0.75f, 0.0f, 0.55f);

    private ICellViewModel _viewModel;
    private CompositeDisposable _disposables;
    private bool _isActive;
    private Vector2Int _coord; // needed for merge animation trigger in BagView

    #region Init (called by BagView)

    public void SetViewModel(ICellViewModel viewModel, Vector2Int coord, bool isActive)
    {
      _coord = coord;
      _isActive = isActive;
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

    #region Binding

    private void Bind()
    {
      // Background color
      _viewModel.BackgroundColor
        .Subscribe(c => _background.color = c)
        .AddTo(_disposables);

      // Icon sprite + visibility
      _viewModel.Icon
        .Subscribe(sprite =>
        {
          if (_iconImage != null) _iconImage.sprite = sprite;
        })
        .AddTo(_disposables);

      _viewModel.IconVisible
        .Subscribe(visible =>
        {
          if (_iconImage != null) _iconImage.enabled = visible;
        })
        .AddTo(_disposables);

      // Highlight overlay
      _viewModel.Highlight
        .Subscribe(ApplyHighlight)
        .AddTo(_disposables);
    }

    #endregion

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

    #endregion

    #region Input → ViewModel commands

    public void OnBeginDrag(PointerEventData eventData) =>
      _viewModel?.OnBeginDrag(eventData.position);

    public void OnDrag(PointerEventData eventData) =>
      _viewModel?.OnDrag(eventData.position);

    public void OnEndDrag(PointerEventData eventData)
    {
      _viewModel?.OnEndDrag();
      PlayPlaceAnimation();
    }

    public void OnDrop(PointerEventData eventData)
    {
      _viewModel?.OnDrop();
      PlayPlaceAnimation();
    }

    public void OnPointerEnter(PointerEventData eventData) =>
      _viewModel?.OnPointerEnter();

    public void OnPointerExit(PointerEventData eventData) =>
      _viewModel?.OnPointerExit();

    #endregion

    #region Animation (purely visual — stays in View)

    public void PlayPlaceAnimation()
    {
      transform.localScale = Vector3.one * 0.8f;
      LeanTween
        .scale(gameObject, Vector3.one, _placeDuration)
        .setEaseOutBack();
    }
    #endregion
  }
}
