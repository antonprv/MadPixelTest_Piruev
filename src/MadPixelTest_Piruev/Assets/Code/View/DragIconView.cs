// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Code.ViewModel.DragIcon;

using R3;

using UnityEngine;
using UnityEngine.UI;

using Zenjex.Extensions.Attribute;
using Zenjex.Extensions.Injector;

namespace Code.View
{
  /// <summary>
  /// MVVM View — floating drag icon.
  ///
  /// Binds to IDragIconViewModel:
  ///   IsVisible  → gameObject.SetActive
  ///   Sprite     → _icon.sprite
  ///   OnPositionUpdate → canvas coordinate conversion + rectTransform.localPosition
  ///
  /// Canvas coordinate math stays here because it requires Unity-specific API
  /// (RectTransformUtility, Canvas.worldCamera) that doesn't belong in a ViewModel.
  /// </summary>
  public class DragIconView : ZenjexBehaviour
  {
    [SerializeField] private Image _icon;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _dragAlpha = 0.85f;

    [Zenjex] private IDragIconViewModel _viewModel;

    private RectTransform _rectTransform;
    private Canvas _rootCanvas;
    private CompositeDisposable _disposables;

    protected override void OnAwake()
    {
      _rectTransform = GetComponent<RectTransform>();
      _rootCanvas = GetComponentInParent<Canvas>();
      _disposables = new CompositeDisposable();

      gameObject.SetActive(false);

      Bind();
    }

    private void OnDestroy() => _disposables?.Dispose();

    private void Bind()
    {
      _viewModel.IsVisible
        .Subscribe(visible =>
        {
          gameObject.SetActive(visible);
          _canvasGroup.alpha = visible ? _dragAlpha : 0f;
        })
        .AddTo(_disposables);

      _viewModel.Sprite
        .Subscribe(sprite =>
        {
          _icon.sprite = sprite;
          _icon.color = sprite != null ? Color.white : Color.clear;
        })
        .AddTo(_disposables);

      _viewModel.OnPositionUpdate
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
