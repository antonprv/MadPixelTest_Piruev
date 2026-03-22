using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace BagFight.Infrastructure.Loading
{
  /// <summary>
  /// Экран загрузки: полупрозрачный оверлей + полоса прогресса.
  /// Инстанциируется из Addressable-префаба в BootstrapState.
  ///
  /// Иерархия префаба:
  ///   LoadingCurtain (CanvasGroup, Image — полный экран)
  ///   └── ProgressBar (Slider)
  ///       ├── Background
  ///       ├── Fill Area / Fill
  /// </summary>
  public class LoadingCurtain : MonoBehaviour, ILoadingScreen
  {
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Slider      _progressBar;

    [SerializeField] private float _fadeDuration = 0.35f;

    private void Awake()
    {
      DontDestroyOnLoad(gameObject);
      _canvasGroup.alpha          = 0f;
      _canvasGroup.blocksRaycasts = false;
      if (_progressBar != null) _progressBar.value = 0f;
    }

    // ─── ILoadingScreen ───────────────────────────────────────────────────────

    public void Show()  => ShowAsync().Forget();
    public void Hide()  => HideAsync().Forget();

    public void SetProgress(float value)
    {
      if (_progressBar != null)
        _progressBar.value = Mathf.Clamp01(value);
    }

    public async UniTask ShowAsync()
    {
      _canvasGroup.blocksRaycasts = true;
      await FadeAsync(0f, 1f, _fadeDuration);
    }

    public async UniTask HideAsync()
    {
      await FadeAsync(1f, 0f, _fadeDuration);
      _canvasGroup.blocksRaycasts = false;
      if (_progressBar != null) _progressBar.value = 0f;
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private async UniTask FadeAsync(float from, float to, float duration)
    {
      float elapsed = 0f;
      while (elapsed < duration)
      {
        elapsed            += Time.unscaledDeltaTime;
        _canvasGroup.alpha  = Mathf.Lerp(from, to, elapsed / duration);
        await UniTask.Yield(PlayerLoopTiming.Update);
      }
      _canvasGroup.alpha = to;
    }
  }
}
