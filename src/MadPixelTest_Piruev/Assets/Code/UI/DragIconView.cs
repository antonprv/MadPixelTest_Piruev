using UnityEngine;
using UnityEngine.UI;

namespace BagFight.UI
{
  /// <summary>
  /// Плавающая иконка, следующая за пальцем / мышью во время drag.
  /// Привязан к корневому Canvas (overlay режим).
  /// Один экземпляр на всю сцену — кладём в installer как singleton.
  /// </summary>
  public class DragIconView : MonoBehaviour
  {
    [SerializeField] private Image        _icon;
    [SerializeField] private CanvasGroup  _canvasGroup;
    [SerializeField] private float        _dragAlpha = 0.85f;

    private RectTransform _rectTransform;
    private Canvas        _rootCanvas;

    private void Awake()
    {
      _rectTransform = GetComponent<RectTransform>();
      _rootCanvas    = GetComponentInParent<Canvas>();
      Hide();
    }

    public void Show(Sprite sprite, Vector2 screenPosition)
    {
      _icon.sprite        = sprite;
      _icon.color         = Color.white;
      _canvasGroup.alpha  = _dragAlpha;
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
