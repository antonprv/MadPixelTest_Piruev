using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using BagFight.Infrastructure.AssetManagement;
using BagFight.Infrastructure.Loading;
using BagFight.Infrastructure.StateMachine.States.Interfaces;

namespace BagFight.Infrastructure.StateMachine
{
  /// <summary>
  /// Стейт 1 из 3.
  ///
  /// Ответственность:
  ///   1. Инициализация Addressables (один раз за сессию)
  ///   2. Спавн LoadingCurtain из Addressable-префаба (DontDestroyOnLoad)
  ///   3. Показ шторки
  ///   4. Переход в PreloadAssetsState
  ///
  /// Почему Addressable-спавн здесь, а не в инсталлере:
  ///   LoadingCurtain нужен ещё до того как загружена основная сцена,
  ///   поэтому создаём его динамически сразу после инициализации Addressables.
  /// </summary>
  public class BootstrapState : IGameState
  {
    public StateType Type => StateType.Bootstrap;

    private readonly IGameStateMachine _gsm;
    private readonly IAssetLoader      _assetLoader;
    private readonly ILoadingScreen    _loadingScreen;

    private CancellationTokenSource _cts;

    public BootstrapState(
      IGameStateMachine gsm,
      IAssetLoader      assetLoader,
      ILoadingScreen    loadingScreen)
    {
      _gsm           = gsm;
      _assetLoader   = assetLoader;
      _loadingScreen = loadingScreen;
    }

    public void Enter() => EnterAsync().Forget();

    private async UniTaskVoid EnterAsync()
    {
      _cts = new CancellationTokenSource();
      var ct = _cts.Token;

      // 1. Инициализируем Addressables
      await _assetLoader.InitializeAsync();

      if (ct.IsCancellationRequested) return;

      // 2. Показываем шторку
      await _loadingScreen.ShowAsync();

      if (ct.IsCancellationRequested) return;

      // 3. Переходим к предзагрузке
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
