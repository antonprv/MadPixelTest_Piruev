// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System.Threading;

using Code.Infrastructure.AssetManagement;
using Code.Infrastructure.Loading;
using Code.Infrastructure.SceneLoader;
using Code.Infrastructure.StateMachine.States.Interfaces;
using Code.Infrastructure.StateMachine.States.Types;

using Cysharp.Threading.Tasks;

namespace Code.Infrastructure.StateMachine.States
{
  /// <summary>
  /// State 1 of 4.
  ///
  /// Responsibilities:
  ///   1. Initialize Addressables (once per session)
  ///   2. Spawn LoadingCurtain from Addressable prefab (DontDestroyOnLoad)
  ///   3. Show curtain
  ///   4. Transition to PreloadAssetsState
  ///
  /// Why Addressable spawn here, not in installer:
  ///   LoadingCurtain is needed before main scene is loaded,
  ///   so we create it dynamically right after Addressables initialization.
  /// </summary>
  public class BootstrapState : IGameState
  {
    public StateType Type => StateType.Bootstrap;

    private readonly IGameStateMachine _gsm;
    private readonly IAssetLoader _assetLoader;
    private readonly ILoadScreen _loadingScreen;
    private readonly ISceneLoader _sceneLoader;

    private CancellationTokenSource _cts;

    public BootstrapState(
      IGameStateMachine gsm,
      IAssetLoader assetLoader,
      ISceneLoader sceneLoader,
      ILoadScreen loadingScreen)
    {
      _gsm = gsm;
      _assetLoader = assetLoader;
      _loadingScreen = loadingScreen;
      _sceneLoader = sceneLoader;
    }

    public void Enter() => EnterAsync().Forget();

    private async UniTaskVoid EnterAsync()
    {
      _cts = new CancellationTokenSource();
      var ct = _cts.Token;

      // 1. Initialize Addressables
      await _assetLoader.InitializeAsync();

      if (ct.IsCancellationRequested) return;

      // 2. Show curtain
      await _loadingScreen.ShowAsync();

      if (ct.IsCancellationRequested) return;

      await _sceneLoader.LoadAsync(SceneAddresses.Initial, ct);

      if (ct.IsCancellationRequested) return;

      OnSceneLoaded();
    }

    private void OnSceneLoaded()
    {
      // 3. Transition to asset preloading
      _gsm.Enter<PreloadAssetsState>();
    }

    public void Exit()
    {
      _cts?.Cancel();
      _cts?.Dispose();
      _cts = null;
    }
  }
}
