using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using BagFight.Infrastructure.AssetsPreloader;
using BagFight.Infrastructure.Loading;
using BagFight.Infrastructure.StateMachine.States.Interfaces;

namespace BagFight.Infrastructure.StateMachine
{
  /// <summary>
  /// Стейт 2 из 3.
  ///
  /// Ответственность:
  ///   1. Параллельная загрузка всех Addressable-иконок через IAssetsPreloader
  ///   2. Обновление прогресс-бара на шторке (R3 Observable → SetProgress)
  ///   3. После завершения — скрытие шторки и переход в GameLoopState
  ///
  /// Паттерн прогресса:
  ///   BagAssetsPreloader пушит float [0..1] в Subject<float>.
  ///   Здесь подписываемся на Observable и пробрасываем в ILoadingScreen.SetProgress.
  ///   Подписка AutoDispose через CancellationToken.
  /// </summary>
  public class PreloadAssetsState : IGameState
  {
    public StateType Type => StateType.PreloadAssets;

    private readonly IGameStateMachine  _gsm;
    private readonly IAssetsPreloader   _preloader;
    private readonly ILoadingScreen     _loadingScreen;

    private CancellationTokenSource _cts;

    public PreloadAssetsState(
      IGameStateMachine  gsm,
      IAssetsPreloader   preloader,
      ILoadingScreen     loadingScreen)
    {
      _gsm           = gsm;
      _preloader     = preloader;
      _loadingScreen = loadingScreen;
    }

    public void Enter() => EnterAsync().Forget();

    private async UniTaskVoid EnterAsync()
    {
      _cts = new CancellationTokenSource();
      var ct = _cts.Token;

      // Подписываемся на прогресс → обновляем полосу загрузки
      using var progressSub = _preloader.Progress
        .Subscribe(v => _loadingScreen.SetProgress(v));

      // Запускаем параллельную загрузку иконок
      // Progress<float> — стандартный IProgress<float>, репортит в тот же Subject
      var progress = new Progress<float>(_ => { }); // Subject уже пушит через Observable
      await _preloader.PreloadItemIconsAsync(progress, ct);

      if (ct.IsCancellationRequested) return;

      // Шторка скрывается — игра готова
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
