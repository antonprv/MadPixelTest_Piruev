// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System.Threading;

using Code.Infrastructure.SceneLoader;
using Code.Infrastructure.StateMachine.States.Interfaces;
using Code.Infrastructure.StateMachine.States.Types;

using Cysharp.Threading.Tasks;

namespace Code.Infrastructure.StateMachine.States
{
  /// <summary>
  /// State 3 of 4 — loads the Main scene via Addressables.
  ///
  /// Responsibilities:
  ///   1. Load "Main" scene (curtain is already visible from PreloadAssetsState)
  ///   2. Transition to GameLoopState
  ///
  /// Why a separate state instead of doing this in PreloadAssetsState:
  ///   Single-responsibility — PreloadAssets owns asset preloading + curtain hide.
  ///   This state owns scene loading. Each state is independently testable
  ///   and can be re-entered (e.g. if we add level restart later).
  ///
  /// Why not load the scene in BootstrapState:
  ///   Scene load must complete after asset preloading — the scene's MonoBehaviours
  ///   (BagView, CellView, etc.) are instantiated by Unity after the scene activates.
  ///   Preloading icons before the scene exists would waste work.
  /// </summary>
  public class LoadLevelState : IGamePayloadedState<string>
  {
    public StateType Type => StateType.LoadLevel;

    private readonly IGameStateMachine _gsm;
    private readonly ISceneLoader _sceneLoader;

    private CancellationTokenSource _cts;

    public LoadLevelState(IGameStateMachine gsm, ISceneLoader sceneLoader)
    {
      _gsm = gsm;
      _sceneLoader = sceneLoader;
    }

    public void Enter(string payload) => EnterAsync(payload).Forget();

    private async UniTaskVoid EnterAsync(string payload)
    {
      _cts = new CancellationTokenSource();
      var ct = _cts.Token;

      await _sceneLoader.LoadAsync(payload, ct);

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
