// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System;
using System.Threading;

using Code.Infrastructure.AssetsPreloader;
using Code.Infrastructure.Loading;
using Code.Infrastructure.StateMachine.States.Interfaces;

using Cysharp.Threading.Tasks;

using R3;

namespace Code.Infrastructure.StateMachine
{
  /// <summary>
  /// State 2 of 3.
  ///
  /// Responsibilities:
  ///   1. Parallel loading of all Addressable icons via IAssetsPreloader
  ///   2. Update progress bar on curtain (R3 Observable → SetProgress)
  ///   3. After completion — hide curtain and transition to GameLoopState
  ///
  /// Progress pattern:
  ///   BagAssetsPreloader pushes float [0..1] to Subject<float>.
  ///   Here we subscribe to Observable and forward to ILoadingScreen.SetProgress.
  ///   Subscription AutoDispose via CancellationToken.
  /// </summary>
  public class PreloadAssetsState : IGameState
  {
    public StateType Type => StateType.PreloadAssets;

    private readonly IGameStateMachine _gsm;
    private readonly IAssetsPreloader _preloader;
    private readonly ILoadingScreen _loadingScreen;

    private CancellationTokenSource _cts;

    public PreloadAssetsState(
      IGameStateMachine gsm,
      IAssetsPreloader preloader,
      ILoadingScreen loadingScreen)
    {
      _gsm = gsm;
      _preloader = preloader;
      _loadingScreen = loadingScreen;
    }

    public void Enter() => EnterAsync().Forget();

    private async UniTaskVoid EnterAsync()
    {
      _cts = new CancellationTokenSource();
      var ct = _cts.Token;

      // Subscribe to progress → update loading bar
      using var progressSub = _preloader.Progress
        .Subscribe(v => _loadingScreen.SetProgress(v));

      // Start parallel icon loading
      // Progress<float> — standard IProgress<float>, reports to same Subject
      var progress = new Progress<float>(_ => { }); // Subject already pushes via Observable
      await _preloader.PreloadItemIconsAsync(progress, ct);

      if (ct.IsCancellationRequested) return;

      // Curtain hides — game is ready
      await _loadingScreen.HideAsync();

      if (ct.IsCancellationRequested) return;

      _gsm.Enter<GameLoopState>();
    }

    public void Exit()
    {
      _cts?.Cancel();
      _cts?.Dispose();
      _cts = null;
    }
  }
}
