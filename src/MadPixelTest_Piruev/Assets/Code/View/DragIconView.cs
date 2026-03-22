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
  /// Receives its ViewModel via Construct() called by UIFactory.
  /// </summary>
  public class DragIconView : MonoBehaviour
  {
    [SerializeField] private Image       _icon;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float       _dragAlpha = 0.85f;

    private RectTransform        _rectTransform;
    private Canvas               _rootCanvas;
    private CompositeDisposable  _disposables;

    /// <summary>Called by UIFactory after domain services are initialized.</summary>
    public void Construct(IDragIconViewModel viewModel)
    {
      _rectTransform = GetComponent<RectTransform>();
      _rootCanvas    = GetComponentInParent<Canvas>();
      _disposables   = new CompositeDisposable();

      gameObject.SetActive(false);

      Bind(viewModel);
    }

    private void OnDestroy() => _disposables?.Dispose();

    private void Bind(IDragIconViewModel viewModel)
    {
      viewModel.IsVisible
        .Subscribe(visible =>
        {
          gameObject.SetActive(visible);
          _canvasGroup.alpha = visible ? _dragAlpha : 0f;
        })
        .AddTo(_disposables);

      viewModel.Sprite
        .Subscribe(sprite =>
        {
          _icon.sprite = sprite;
          _icon.color  = sprite != null ? Color.white : Color.clear;
        })
        .AddTo(_disposables);

      viewModel.OnPositionUpdate
        .Subscribe(UpdateCanvasPosition)
        .AddTo(_disposables);
    }

    private void UpdateCanvasPosition(Vector2 screenPosition)
    {
      RectTransformUtility.ScreenPointToLocalPointInRectangle(
        _rootCanvas.GetComponent<RectTransform>(),
        screenPosition,
        _rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _rootCanvas.worldCamera,
        out var localPoint);

      _rectTransform.localPosition = localPoint;
    }
  }
}
