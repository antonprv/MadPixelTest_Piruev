// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections;

using Code.Data.StaticData;
using Code.Infrastructure.AssetManagement;
using Code.Infrastructure.AssetsPreloader;
using Code.Infrastructure.Loading;
using Code.Infrastructure.StateMachine;
using Code.Infrastructure.StateMachine.Factory;
using Code.Services;
using Code.UI;

using Reflex.Core;

using UnityEngine;

using Zenjex.Extensions.Core;

namespace Code.Infrastructure
{
  /// <summary>
  /// Root DI installer of the project.
  ///
  /// Registration order:
  ///   1. Data (ScriptableObjects)
  ///   2. Asset infrastructure (AssetLoader, AssetsPreloader)
  ///   3. Loading UI (LoadingCurtain)
  ///   4. GSM — GameStateMachine + StateFactory + all states
  ///   5. Gameplay services (GridInventory, BottomSlots, DragDrop)
  ///   6. Scene singletons (DragIconView)
  ///
  /// GameStateMachine implements IInitializable —
  ///   Zenjex calls Initialize() after container assembly —
  ///   GSM enters BootstrapState automatically.
  /// </summary>
  public class BagInstaller : ProjectRootInstaller
  {
    [Header("Configs")]
    [SerializeField] private BagConfig _bagConfig;
    [SerializeField] private ItemManifest _itemManifest;

    [Header("Scene references")]
    [SerializeField] private DragIconView _dragIconView;
    [SerializeField] private LoadingCurtain _loadingCurtain;

    public override void InstallBindings(ContainerBuilder builder)
    {
      RegisterData(builder);
      RegisterAssetInfrastructure(builder);
      RegisterLoadingUI(builder);
      RegisterGSM(builder);
      RegisterGameplayServices(builder);
      RegisterSceneSingletons(builder);
    }

    public override IEnumerator InstallGameInstanceRoutine() => null;

    // LaunchGame is empty — start via IInitializable on GameStateMachine
    public override void LaunchGame() { }

    // ─── Registration groups ──────────────────────────────────────────────────

    private void RegisterData(ContainerBuilder builder)
    {
      builder.BindInstance(_bagConfig).AsSingle();
      builder.BindInstance(_itemManifest).AsSingle();
    }

    private void RegisterAssetInfrastructure(ContainerBuilder builder)
    {
      builder.Bind<AssetLoader>()
        .BindInterfacesAndSelf()   // → IAssetLoader
        .AsSingle();

      builder.Bind<BagAssetsPreloader>()
        .BindInterfacesAndSelf()   // → IAssetsPreloader
        .AsSingle();
    }

    private void RegisterLoadingUI(ContainerBuilder builder)
    {
      builder.BindInstance(_loadingCurtain)
        .BindInterfacesAndSelf()   // → ILoadingScreen + LoadingCurtain
        .AsSingle();
    }

    private void RegisterGSM(ContainerBuilder builder)
    {
      // StateFactory resolves states from container
      builder.Bind<StateFactory>()
        .AsSingle();

      // GameStateMachine — IInitializable → Zenjex calls Initialize() automatically
      builder.Bind<GameStateMachine>()
        .BindInterfacesAndSelf()   // → IGameStateMachine + IInitializable
        .AsSingle();

      // States — AsTransient: fresh instance on each transition
      builder.Bind<BootstrapState>().AsTransient();
      builder.Bind<PreloadAssetsState>().AsTransient();
      builder.Bind<GameLoopState>().AsTransient();
    }

    private void RegisterGameplayServices(ContainerBuilder builder)
    {
      builder.Bind<GridInventoryService>()
        .BindInterfacesAndSelf()   // → IGridInventoryService + IInitializable
        .AsSingle();

      builder.Bind<BottomSlotsService>()
        .BindInterfacesAndSelf()   // → IBottomSlotsService + IInitializable
        .AsSingle();

      builder.Bind<GridDragDropService>()
        .BindInterfacesAndSelf()   // → IGridDragDropService
        .AsSingle();
    }

    private void RegisterSceneSingletons(ContainerBuilder builder)
    {
      builder.BindInstance(_dragIconView).AsSingle();
    }
  }
}
