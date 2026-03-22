// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Common.FastMath;

using Cysharp.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

namespace Code.Infrastructure.Loading
{
  /// <summary>
  /// Loading screen: semi-transparent overlay + progress bar.
  /// Created together with GameInstance, this is a singleton
  ///
  /// Prefab hierarchy:
  ///   LoadingCurtain (CanvasGroup, Image — full screen)
  ///   └── ProgressBar (Slider)
  ///       ├── Background
  ///       ├── Fill Area / Fill
  /// </summary>
  public class LoadingCurtain : MonoBehaviour, ILoadScreen
  {
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Slider _progressBar;

    [SerializeField] private float _fadeDuration = 0.35f;

    private void Awake()
    {
      DontDestroyOnLoad(gameObject);
      _canvasGroup.alpha = 0f;
      _canvasGroup.blocksRaycasts = false;
      if (_progressBar != null) _progressBar.value = 0f;
    }

    #region ILoadingScreen

    public void Show() => ShowAsync().Forget();
    public void Hide() => HideAsync().Forget();

    public void SetProgress(float value)
    {
      if (_progressBar != null)
        _progressBar.value = FMath.Clamp01(value);
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

    #endregion


    #region Helpers

    private async UniTask FadeAsync(float from, float to, float duration)
    {
      float elapsed = 0f;
      while (elapsed < duration)
      {
        elapsed += Time.unscaledDeltaTime;
        _canvasGroup.alpha = FMath.Lerp(from, to, elapsed / duration);
        await UniTask.Yield(PlayerLoopTiming.Update);
      }
      _canvasGroup.alpha = to;
    }

    #endregion
  }
}
