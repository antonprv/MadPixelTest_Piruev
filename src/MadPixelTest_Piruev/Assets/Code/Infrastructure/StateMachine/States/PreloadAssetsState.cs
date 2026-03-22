// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System;
using System.Threading;

using Code.Infrastructure.AssetsPreloader;
using Code.Infrastructure.Loading;
using Code.Infrastructure.SceneLoader;
using Code.Infrastructure.Services.StaticData;
using Code.Infrastructure.StateMachine.States.Interfaces;
using Code.Infrastructure.StateMachine.States.Types;

using Cysharp.Threading.Tasks;

using R3;

namespace Code.Infrastructure.StateMachine.States
{
  /// <summary>
  /// State 2 of 4.
  ///
  /// Responsibilities:
  ///   1. Load all static data (BagConfig, ItemManifest) via IStaticDataService
  ///   2. Parallel loading of all Addressable icons via IAssetsPreloader
  ///   3. Update progress bar on curtain (R3 Observable → SetProgress)
  ///   4. After completion — hide curtain and transition to LoadLevelState
  ///
  /// Order matters: LoadAllAsync must complete before PreloadItemIconsAsync,
  /// because the preloader reads IItemDataSubservice.Items which is populated
  /// by StaticDataService.
  /// </summary>
  public class PreloadAssetsState : IGameState
  {
    public StateType Type => StateType.PreloadAssets;

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

      // 1. Load manifests and configs — must finish before icon preloading
      await _staticData.LoadAllAsync();

      if (ct.IsCancellationRequested) return;

      // 2. Subscribe to progress → update loading bar
      using var progressSub = _preloader.Progress
        .Subscribe(v => _loadingScreen.SetProgress(v));

      var progress = new Progress<float>(_ => { });
      await _preloader.PreloadItemIconsAsync(progress, ct);

      if (ct.IsCancellationRequested) return;

      await _loadingScreen.HideAsync();

      if (ct.IsCancellationRequested) return;

      _gsm.Enter<LoadLevelState, string>(SceneAddresses.Main);
    }

    public void Exit()
    {
      _cts?.Cancel();
      _cts?.Dispose();
      _cts = null;
    }
  }
}
