using System.Collections;
using Reflex.Core;
using UnityEngine;
using BagFight.Data;
using BagFight.Infrastructure.AssetManagement;
using BagFight.Infrastructure.AssetsPreloader;
using BagFight.Infrastructure.Loading;
using BagFight.Infrastructure.StateMachine;
using BagFight.Infrastructure.StateMachine.Factory;
using BagFight.Services;
using BagFight.Services.Interfaces;
using BagFight.UI;
using Zenjex.Extensions.Core;

namespace BagFight.Infrastructure
{
  /// <summary>
  /// Корневой DI-инсталлер проекта.
  ///
  /// Порядок регистраций:
  ///   1. Данные (ScriptableObjects)
  ///   2. Asset infrastructure (AssetLoader, AssetsPreloader)
  ///   3. Loading UI (LoadingCurtain)
  ///   4. GSM — GameStateMachine + StateFactory + все стейты
  ///   5. Gameplay-сервисы (GridInventory, BottomSlots, DragDrop)
  ///   6. Scene-синглтоны (DragIconView)
  ///
  /// GameStateMachine реализует IInitializable →
  ///   Zenjex вызовет Initialize() после сборки контейнера →
  ///   GSM войдёт в BootstrapState автоматически.
  /// </summary>
  public class BagInstaller : ProjectRootInstaller
  {
    [Header("Configs")]
    [SerializeField] private BagConfig    _bagConfig;
    [SerializeField] private ItemManifest _itemManifest;

    [Header("Scene references")]
    [SerializeField] private DragIconView   _dragIconView;
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

    // LaunchGame пустой — старт через IInitializable на GameStateMachine
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
      // StateFactory резолвит стейты из контейнера
      builder.Bind<StateFactory>()
        .AsSingle();

      // GameStateMachine — IInitializable → Zenjex запустит Initialize() автоматически
      builder.Bind<GameStateMachine>()
        .BindInterfacesAndSelf()   // → IGameStateMachine + IInitializable
        .AsSingle();

      // Стейты — AsTransient: свежий экземпляр при каждом переходе
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
