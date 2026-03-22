// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
  /// <summary>
  /// Floating icon following finger/mouse during drag.
  /// Attached to root Canvas (overlay mode).
  /// Single instance per scene — register in installer as singleton.
  /// </summary>
  public class DragIconView : MonoBehaviour
  {
    [SerializeField] private Image _icon;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _dragAlpha = 0.85f;

    private RectTransform _rectTransform;
    private Canvas _rootCanvas;

    private void Awake()
    {
      _rectTransform = GetComponent<RectTransform>();
      _rootCanvas = GetComponentInParent<Canvas>();
      Hide();
    }

    public void Show(Sprite sprite, Vector2 screenPosition)
    {
      _icon.sprite = sprite;
      _icon.color = Color.white;
      _canvasGroup.alpha = _dragAlpha;
      gameObject.SetActive(true);
      UpdatePosition(screenPosition);
    }

    public void UpdatePosition(Vector2 screenPosition)
    {
      RectTransformUtility.ScreenPointToLocalPointInRectangle(
        _rootCanvas.GetComponent<RectTransform>(),
        screenPosition,
        _rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _rootCanvas.worldCamera,
        out var localPoint
      );
      _rectTransform.localPosition = localPoint;
    }

    public void Hide()
    {
      gameObject.SetActive(false);
      _icon.sprite = null;
    }
  }
}
