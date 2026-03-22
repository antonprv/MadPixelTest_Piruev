// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Code.ViewModel.DragIcon;

using R3;

using UnityEngine;
using UnityEngine.UI;

namespace Code.View
{
  /// <summary>
  /// MVVM View — floating drag icon.
  ///
  /// Two new behaviours vs previous version:
  ///
  /// 1. Icon size = ½ of the item's grid bounding box.
  ///    DragIconViewModel.DragItemBounds carries the item's cell footprint.
  ///    BagView exposes the computed step/spacing so we can convert to pixels.
  ///    Size is applied in Show (via IsVisible subscription).
  ///
  /// 2. Fly-to animation on void-drop / cancel.
  ///    DragIconViewModel.OnFlyTo fires with the target slot's screen position.
  ///    The View animates the RectTransform to that position, shrinks the icon,
  ///    then calls Hide(). This gives the "item flies back to slot" feel.
  /// </summary>
  public class DragIconView : MonoBehaviour
  {
    [SerializeField] private Image       _icon;
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("Drag")]
    [SerializeField] private float _dragAlpha    = 0.85f;

    [Header("Fly-back animation")]
    [SerializeField] private float _flyDuration  = 0.25f;
    [SerializeField] private float _flyEndScale  = 0.4f;

    private IDragIconViewModel _viewModel;
    private RectTransform      _rectTransform;
    private Canvas             _rootCanvas;
    private CompositeDisposable _disposables;

    // Injected by UIFactory so we can compute pixel size from grid metrics
    private float     _gridCellStep;    // cellSize + spacing
    private float     _gridSpacing;

    /// <summary>Called by UIFactory.</summary>
    public void Construct(IDragIconViewModel viewModel, float gridCellStep, float gridSpacing)
    {
      _viewModel     = viewModel;
      _gridCellStep  = gridCellStep;
      _gridSpacing   = gridSpacing;

      _rectTransform = GetComponent<RectTransform>();
      _rootCanvas    = GetComponentInParent<Canvas>();
      _disposables   = new CompositeDisposable();

      gameObject.SetActive(false);

      Bind();
    }

    private void OnDestroy() => _disposables?.Dispose();

    private void Bind()
    {
      _viewModel.IsVisible
        .Subscribe(visible =>
        {
          if (visible)
          {
            // Size icon at ½ of the item's bounding box in grid pixels
            var bounds = _viewModel.DragItemBounds;
            float w = bounds.x * _gridCellStep - _gridSpacing;
            float h = bounds.y * _gridCellStep - _gridSpacing;
            _rectTransform.sizeDelta = new Vector2(w, h) * 0.5f;

            transform.localScale     = Vector3.one;
            _canvasGroup.alpha       = _dragAlpha;
            gameObject.SetActive(true);
          }
          else
          {
            // Immediate hide (used when dropped successfully)
            gameObject.SetActive(false);
            _canvasGroup.alpha = 0f;
          }
        })
        .AddTo(_disposables);

      _viewModel.Sprite
        .Subscribe(sprite =>
        {
          _icon.sprite = sprite;
          _icon.color  = sprite != null ? Color.white : Color.clear;
        })
        .AddTo(_disposables);

      _viewModel.OnPositionUpdate
        .Subscribe(UpdateCanvasPosition)
        .AddTo(_disposables);

      _viewModel.OnFlyTo
        .Subscribe(FlyToPosition)
        .AddTo(_disposables);
    }

    private void UpdateCanvasPosition(Vector2 screenPosition)
    {
      if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rootCanvas.GetComponent<RectTransform>(),
            screenPosition,
            _rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
              ? null
              : _rootCanvas.worldCamera,
            out var localPoint)) return;

      _rectTransform.localPosition = localPoint;
    }

    private void FlyToPosition(Vector2 targetScreenPosition)
    {
      // Convert target to canvas local position
      RectTransformUtility.ScreenPointToLocalPointInRectangle(
        _rootCanvas.GetComponent<RectTransform>(),
        targetScreenPosition,
        _rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
          ? null
          : _rootCanvas.worldCamera,
        out var targetLocal);

      // Cancel any running tween on this object
      LeanTween.cancel(gameObject);

      // Move to target slot position
      LeanTween
        .moveLocal(gameObject, targetLocal, _flyDuration)
        .setEaseInBack();

      // Shrink and fade out simultaneously
      LeanTween
        .scale(gameObject, Vector3.one * _flyEndScale, _flyDuration)
        .setEaseInBack();

      LeanTween
        .value(gameObject, _dragAlpha, 0f, _flyDuration)
        .setOnUpdate((float v) => _canvasGroup.alpha = v)
        .setOnComplete(() =>
        {
          _viewModel.Hide();
          transform.localScale = Vector3.one;
        });
    }
  }
}
