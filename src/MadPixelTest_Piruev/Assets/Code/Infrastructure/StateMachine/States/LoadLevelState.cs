// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System.Threading;

using Code.Infrastructure.SceneLoader;
using Code.Infrastructure.StateMachine.States.Interfaces;
using Code.Infrastructure.StateMachine.States.Types;
using Code.Model.Services.BottomSlots.Interfaces;
using Code.Model.Services.Inventory.Interfaces;
using Code.UI.Factory;

using Cysharp.Threading.Tasks;

namespace Code.Infrastructure.StateMachine.States
{
  /// <summary>
  /// State 3 of 4.
  ///
  /// Exact order matters:
  ///   1. InitializeModelServices() — grid and slots arrays are allocated.
  ///      Must be first: ViewModels read from services the moment they are created.
  ///   2. LoadAsync() — loads the Main scene. Scene Awake() fires here,
  ///      but BagView / BottomSlotsView / DragIconView are plain MonoBehaviours
  ///      with empty Awake — they wait for Construct().
  ///   3. CreateGameplayUIAsync() — factory loads BagCanvas from Addressables,
  ///      instantiates it, then calls Construct(viewModel) on each View.
  ///      At this point services are ready, so ViewModel constructors are safe.
  ///   4. GameLoopState.
  /// </summary>
  public class LoadLevelState : IGamePayloadedState<string>
  {
    public StateType Type => StateType.LoadLevel;

    private readonly IGameStateMachine    _gsm;
    private readonly ISceneLoader         _sceneLoader;
    private readonly IGridInventoryService _gridInventory;
    private readonly IBottomSlotsService   _bottomSlots;
    private readonly IUIFactory           _uiFactory;

    private CancellationTokenSource _cts;

    public LoadLevelState(
      IGameStateMachine     gsm,
      ISceneLoader          sceneLoader,
      IGridInventoryService gridInventory,
      IBottomSlotsService   bottomSlots,
      IUIFactory            uiFactory)
    {
      _gsm           = gsm;
      _sceneLoader   = sceneLoader;
      _gridInventory = gridInventory;
      _bottomSlots   = bottomSlots;
      _uiFactory     = uiFactory;
    }

    public void Enter(string payload) => EnterAsync(payload).Forget();

    private async UniTaskVoid EnterAsync(string payload)
    {
      _cts = new CancellationTokenSource();
      var ct = _cts.Token;

      // 1. Initialize domain services before anything touches them
      InitializeModelServices();

      // 2. Load the scene — Views Awake() fires here but does nothing
      //    (no ZenjexBehaviour, no [Zenjex] fields, empty MonoBehaviour)
      await _sceneLoader.LoadAsync(payload, ct);

      if (ct.IsCancellationRequested) return;

      // 3. Create and wire UI — safe because services are already initialized
      await _uiFactory.CreateGameplayUIAsync();

      if (ct.IsCancellationRequested) return;

      _gsm.Enter<GameLoopState>();
    }

    private void InitializeModelServices()
    {
      _gridInventory.Initialize();
      _bottomSlots.Initialize();
    }

    public void Exit()
    {
      _cts?.Cancel();
      _cts?.Dispose();
      _cts = null;
    }
  }
}
