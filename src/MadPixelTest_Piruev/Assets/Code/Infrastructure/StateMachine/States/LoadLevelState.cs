// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System.Threading;

using Code.Infrastructure.Loading;
using Code.Infrastructure.SceneLoader;
using Code.Infrastructure.Services.StaticData;
using Code.Infrastructure.StateMachine.States.Interfaces;
using Code.Infrastructure.StateMachine.States.Types;
using Code.Model.Services.BottomSlots.Interfaces;
using Code.Model.Services.Inventory.Interfaces;
using Code.UI.Factory;
using Code.UI.Hud;

using Cysharp.Threading.Tasks;

namespace Code.Infrastructure.StateMachine.States
{
  /// <summary>
  /// State 4 of 5.
  ///
  /// Progress split:
  ///   0.00 → 0.10  WarmUp (prefab cache)
  ///   0.10 → 0.30  LoadForLevelAsync (resolve BagConfig + ItemPreset)
  ///   0.30 → 0.35  InitializeModelServices (sync)
  ///   0.35 → 0.80  LoadAsync scene (Addressables PercentComplete → mapped range)
  ///   0.80 → 0.95  CreateUIRoot + CreateGameplayUIAsync + CreateHudAsync
  ///   0.95 → 1.00  HideAsync curtain
  /// </summary>
  public class LoadLevelState : IGamePayloadedState<string>
  {
    public StateType Type => StateType.LoadLevel;

    private readonly IGameStateMachine     _gsm;
    private readonly ISceneLoader          _sceneLoader;
    private readonly IStaticDataService    _staticData;
    private readonly IGridInventoryService _gridInventory;
    private readonly IBottomSlotsService   _bottomSlots;
    private readonly IUIFactory            _uiFactory;
    private readonly ILoadScreen           _loadingScreen;

    private CancellationTokenSource _cts;
    private HudView                  _hudView;

    public LoadLevelState(
      IGameStateMachine     gsm,
      ISceneLoader          sceneLoader,
      IStaticDataService    staticData,
      IGridInventoryService gridInventory,
      IBottomSlotsService   bottomSlots,
      IUIFactory            uiFactory,
      ILoadScreen           loadingScreen)
    {
      _gsm           = gsm;
      _sceneLoader   = sceneLoader;
      _staticData    = staticData;
      _gridInventory = gridInventory;
      _bottomSlots   = bottomSlots;
      _uiFactory     = uiFactory;
      _loadingScreen = loadingScreen;
    }

    public void Enter(string levelName) => EnterAsync(levelName).Forget();

    private async UniTaskVoid EnterAsync(string levelName)
    {
      _cts = new CancellationTokenSource();
      var ct = _cts.Token;

      _loadingScreen.SetProgress(0f);
      await _loadingScreen.ShowAsync();
      if (ct.IsCancellationRequested) return;

      // 0 → 10 % — warm up prefab cache
      await _uiFactory.WarmUp();
      if (ct.IsCancellationRequested) return;
      _loadingScreen.SetProgress(0.10f);

      // 10 → 30 % — resolve per-level BagConfig and ItemPreset
      await _staticData.LevelData.LoadForLevelAsync(levelName);
      if (ct.IsCancellationRequested) return;
      _loadingScreen.SetProgress(0.30f);

      // 30 → 35 % — initialise domain services
      //   IBagConfigSubservice now reads live from LevelStaticDataService.CurrentBagConfig,
      //   so no explicit refresh needed before Initialize().
      InitializeModelServices();
      _loadingScreen.SetProgress(0.35f);

      // 35 → 80 % — load scene (sceneProgress properly forwarded to ISceneLoader)
      var sceneProgress = new System.Progress<float>(v =>
        _loadingScreen.SetProgress(0.35f + v * 0.45f));

      await _sceneLoader.LoadAsync(levelName, ct, sceneProgress);
      if (ct.IsCancellationRequested) return;
      _loadingScreen.SetProgress(0.80f);

      // 80 → 95 % — create UI
      _uiFactory.CreateUIRoot();
      await _uiFactory.CreateGameplayUIAsync();
      if (ct.IsCancellationRequested) return;

      _hudView = await _uiFactory.CreateHudAsync();
      if (ct.IsCancellationRequested) return;
      _loadingScreen.SetProgress(0.95f);

      // 95 → 100 % — hide curtain
      await _loadingScreen.HideAsync();
      if (ct.IsCancellationRequested) return;
      _loadingScreen.SetProgress(1.00f);

      _gsm.Enter<GameLoopState, HudView>(_hudView);
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
