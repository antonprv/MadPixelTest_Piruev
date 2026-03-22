// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections;

using Code.Common.Extensions.Async;
using Code.Common.Extensions.Logging;
using Code.Infrastructure.AssetManagement;
using Code.Infrastructure.AssetsPreloader;
using Code.Infrastructure.Installer.Factory;
using Code.Infrastructure.Loading;
using Code.Infrastructure.SceneLoader;
using Code.Infrastructure.Services.StaticData;
using Code.Infrastructure.Services.StaticData.Interfaces;
using Code.Infrastructure.Services.StaticData.Subservices;
using Code.Infrastructure.Services.Time;
using Code.Infrastructure.StateMachine;
using Code.Infrastructure.StateMachine.Factory;
using Code.Infrastructure.StateMachine.States;
using Code.Model.Services.BottomSlots;
using Code.Model.Services.DragDrop;
using Code.Model.Services.Inventory;
using Code.Model.Services.Startup;
using Code.Presenter.Bag;
using Code.Presenter.BottomSlots;
using Code.Presenter.DragDrop;
using Code.UI.Factory;
using Code.ViewModel.Bag;
using Code.ViewModel.BottomSlots;
using Code.ViewModel.DragIcon;

using Reflex.Core;

using Zenjex.Extensions.Core;

namespace Code.Infrastructure.Installer
{
  public class GameInstaller : ProjectRootInstaller
  {
    private GameInstance _gameInstance;
    private ILoadScreen  _loadScreen;

    #region Lifecycle

    public override IEnumerator InstallGameInstanceRoutine()
    {
      yield return InstallerFactory.CreateLoadingScreenRoutine(screen =>
        _loadScreen = screen);

      RootContainer.Bind<ILoadScreen>()
        .FromInstance(_loadScreen)
        .AsSingle();

      yield return InstallerFactory.CreateGameInstanceRoutine(
        onBeforeActivate: instance =>
        {
          _gameInstance = instance;
          RootContainer.Bind<ICoroutineRunner>()
            .FromInstance(instance)
            .AsSingle();
        });
    }

    #endregion

    public override void InstallBindings(ContainerBuilder builder)
    {
      BindLogging(builder);
      BindAssetManagement(builder);
      BindSceneLoader(builder);
      BindUnityServices(builder);
      BindStaticData(builder);
      BindDomainServices(builder);
      BindPresenters(builder);
      BindViewModels(builder);
      BindUI(builder);
      BindGSM(builder);
    }

    public override void LaunchGame() => _gameInstance.LaunchGame();

    #region Infrastructure

    private void BindLogging(ContainerBuilder builder) =>
      builder.Bind<IGameLog>().To<GameLogger>().AsSingle();

    private void BindAssetManagement(ContainerBuilder builder)
    {
      builder.Bind<IAssetLoader>().To<AssetLoader>().AsSingle();
      builder.Bind<IAssetsPreloader>().To<AddressableAssetPreloader>().AsSingle();
    }

    private void BindSceneLoader(ContainerBuilder builder) =>
      builder.Bind<ISceneLoader>().To<AddressableSceneLoader>().AsSingle();

    private void BindUnityServices(ContainerBuilder builder) =>
      builder.Bind<ITimeService>().To<UnityTimeService>().AsSingle();

    #endregion

    #region Static data

    private void BindStaticData(ContainerBuilder builder)
    {
      // Per-level bag layout + startup items — loaded per level in LoadLevelState.
      // Registered first because LevelBagConfigSubservice depends on it.
      builder
        .Bind<ILevelStaticDataService>()
        .To<LevelStaticDataService>()
        .AsSingle();

      // IBagConfigSubservice delegates live to ILevelStaticDataService.CurrentBagConfig.
      // No global BagConfig address is loaded — each level brings its own.
      builder
        .Bind<IBagConfigSubservice>()
        .To<LevelBagConfigSubservice>()
        .AsSingle();

      // Global item catalogue — loaded once in PreloadAssetsState.
      builder
        .Bind<IItemDataSubservice>()
        .To<ItemDataSubservice>()
        .AsSingle();

      builder
        .Bind<IStaticDataService>()
        .To<StaticDataService>()
        .AsSingle();
    }

    #endregion

    #region Domain services

    private void BindDomainServices(ContainerBuilder builder)
    {
      builder.Bind<GridInventoryService>()
        .BindInterfacesAndSelf()
        .AsSingle();

      builder.Bind<BottomSlotsService>()
        .BindInterfacesAndSelf()
        .AsSingle();

      builder.Bind<GridDragDropService>()
        .BindInterfacesAndSelf()
        .AsSingle();

      builder.Bind<IStartupItemsService>().To<StartupItemsService>().AsSingle();
    }

    #endregion

    #region MVP Presenters

    private void BindPresenters(ContainerBuilder builder)
    {
      builder.Bind<IBagPresenter>().To<BagPresenter>().AsSingle();
      builder.Bind<IBottomSlotsPresenter>().To<BottomSlotsPresenter>().AsSingle();
      builder.Bind<IDragDropPresenter>().To<DragDropPresenter>().AsSingle();
    }

    #endregion

    #region MVVM ViewModels

    private void BindViewModels(ContainerBuilder builder)
    {
      // DragIconViewModel is stateless between levels — keep as singleton.
      builder.Bind<IDragIconViewModel>().To<DragIconViewModel>().AsSingle();

      // BagViewModel and BottomSlotsViewModel cache IBagConfigSubservice values
      // (GridSize, ActiveCells, BottomSlotCount) in their constructors.
      // Because IBagConfigSubservice is level-specific, these ViewModels must be
      // recreated on every level load so they read the correct level's config.
      // UIFactory resolves them fresh via Container.Resolve<T>() each time.
      builder.Bind<IBagViewModel>().To<BagViewModel>().AsTransient();
      builder.Bind<IBottomSlotsViewModel>().To<BottomSlotsViewModel>().AsTransient();
    }

    #endregion

    #region UI

    private void BindUI(ContainerBuilder builder) =>
      builder.Bind<IUIFactory>().To<UIFactory>().AsSingle();

    #endregion

    #region GSM

    private void BindGSM(ContainerBuilder builder)
    {
      builder.Bind<StateFactory>().AsSingle();
      builder.Bind<IGameStateMachine>().To<GameStateMachine>().AsSingle();

      builder.Bind<BootstrapState>().AsTransient();
      builder.Bind<PreloadAssetsState>().AsTransient();
      builder.Bind<MainMenuState>().AsTransient();
      builder.Bind<LoadLevelState>().AsTransient();
      builder.Bind<GameLoopState>().AsTransient();
    }

    #endregion
  }
}
