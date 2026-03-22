// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System;
using System.Threading;

using Code.Infrastructure.AssetsPreloader;
using Code.Infrastructure.Loading;
using Code.Infrastructure.Services.StaticData;
using Code.Infrastructure.StateMachine.States.Interfaces;
using Code.Infrastructure.StateMachine.States.Types;

using Cysharp.Threading.Tasks;

using R3;

namespace Code.Infrastructure.StateMachine.States
{
  /// <summary>
  /// State 2 of 5.
  ///
  /// Responsibilities:
  ///   1. Load all static data (BagConfig, ItemManifest, Level manifests)
  ///   2. Preload all Addressable icons
  ///   3. Track progress [0..1] on the loading curtain
  ///   4. Hide curtain → transition to MainMenuState
  ///
  /// Progress split:
  ///   0 → 0.3  static data  (LoadAllAsync — fast, mostly network/disk)
  ///   0.3 → 1  icon preload (IAssetsPreloader.Progress mapped to 0.3..1)
  /// </summary>
  public class PreloadAssetsState : IGameState
  {
    public StateType Type => StateType.PreloadAssets;

    private const float StaticDataWeight = 0.3f;

    private readonly IGameStateMachine  _gsm;
    private readonly IStaticDataService _staticData;
    private readonly IAssetsPreloader   _preloader;
    private readonly ILoadScreen        _loadingScreen;

    private CancellationTokenSource _cts;

    public PreloadAssetsState(
      IGameStateMachine  gsm,
      IStaticDataService staticData,
      IAssetsPreloader   preloader,
      ILoadScreen        loadingScreen)
    {
      _gsm           = gsm;
      _staticData    = staticData;
      _preloader     = preloader;
      _loadingScreen = loadingScreen;
    }

    public void Enter() => EnterAsync().Forget();

    private async UniTaskVoid EnterAsync()
    {
      _cts = new CancellationTokenSource();
      var ct = _cts.Token;

      // 1. Static data — 0..30 %
      _loadingScreen.SetProgress(0f);
      await _staticData.LoadAllAsync();
      if (ct.IsCancellationRequested) return;

      _loadingScreen.SetProgress(StaticDataWeight);

      // 2. Icon preload — 30..100 %
      //    Map IAssetsPreloader.Progress [0..1] → [0.3..1.0]
      using var progressSub = _preloader.Progress
        .Subscribe(v =>
          _loadingScreen.SetProgress(StaticDataWeight + v * (1f - StaticDataWeight)));

      await _preloader.PreloadItemIconsAsync(new Progress<float>(_ => { }), ct);
      if (ct.IsCancellationRequested) return;

      _loadingScreen.SetProgress(1f);

      // 3. Hide curtain then show main menu
      await _loadingScreen.HideAsync();
      if (ct.IsCancellationRequested) return;

      _gsm.Enter<MainMenuState>();
    }

    public void Exit()
    {
      _cts?.Cancel();
      _cts?.Dispose();
      _cts = null;
    }
  }
}
