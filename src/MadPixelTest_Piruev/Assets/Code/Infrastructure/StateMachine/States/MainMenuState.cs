// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System.Threading;

using Code.Common.Extensions.Logging;
using Code.Infrastructure.SceneLoader;
using Code.Infrastructure.StateMachine.States.Interfaces;
using Code.Infrastructure.StateMachine.States.Types;
using Code.UI.MainMenu;

using Cysharp.Threading.Tasks;

using UnityEngine;

namespace Code.Infrastructure.StateMachine.States
{
  /// <summary>
  /// State 3 of 5 — Main Menu.
  ///
  /// On Enter:
  ///   1. Load the MainMenu scene
  ///   2. Find MainMenuView in the loaded scene
  ///   3. Subscribe to its level/quit buttons
  ///
  /// UI_Root cleanup:
  ///   GameLoopState.OnReturnClicked calls UIFactory.Cleanup() before
  ///   transitioning here, so UIRoot is already destroyed. This state
  ///   never needs to call Cleanup() itself.
  /// </summary>
  public class MainMenuState : IGameState
  {
    public StateType Type => StateType.MainMenu;

    private readonly IGameStateMachine _gsm;
    private readonly ISceneLoader _sceneLoader;
    private readonly IGameLog _logger;
    private CancellationTokenSource _cts;
    private MainMenuView _view;

    public MainMenuState(
      IGameStateMachine gsm,
      ISceneLoader sceneLoader,
      IGameLog gameLog
      )
    {
      _gsm = gsm;
      _sceneLoader = sceneLoader;
      _logger = gameLog;
    }

    public void Enter() => EnterAsync().Forget();

    private async UniTaskVoid EnterAsync()
    {
      _cts = new CancellationTokenSource();
      var ct = _cts.Token;

      await _sceneLoader.LoadAsync(SceneAddresses.MainMenuAddress, ct);
      if (ct.IsCancellationRequested) return;

      OnMainMenuLoaded();
    }

    private void OnMainMenuLoaded()
    {
      _view = Object.FindAnyObjectByType<MainMenuView>();

      if (_view == null)
      {
        _logger.Log(LogType.Error, "MainMenuView not found in MainMenu scene.");
        return;
      }

      _view.OnLevel1Clicked += PlayLevel1;
      _view.OnLevel2Clicked += PlayLevel2;
    }

    public void Exit()
    {
      if (_view != null)
      {
        _view.OnLevel1Clicked -= PlayLevel1;
        _view.OnLevel2Clicked -= PlayLevel2;
      }

      _cts?.Cancel();
      _cts?.Dispose();
      _cts = null;
    }

    private void PlayLevel1() =>
      _gsm.Enter<LoadLevelState, string>(SceneAddresses.Level1Address);

    private void PlayLevel2() =>
      _gsm.Enter<LoadLevelState, string>(SceneAddresses.Level2Address);
  }
}
