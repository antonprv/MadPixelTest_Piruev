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
using Code.Infrastructure.Services.Time;
using Code.Infrastructure.StateMachine;
using Code.Infrastructure.StateMachine.Factory;
using Code.Infrastructure.StateMachine.States;
using Code.UI.Services.BottomSlots;
using Code.UI.Services.DragDrop;
using Code.UI.Services.Inventory;

using Reflex.Core;

using Zenjex.Extensions.Core;

namespace Code.Infrastructure.Installer
{
  /// <summary>
  /// Root DI installer of the project.
  /// </summary>
  public class GameInstaller : ProjectRootInstaller
  {
    private GameInstance _gameInstance;
    private ILoadScreen _loadScreen;

    #region Game Instance Setup

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
        BindGameInstanceComponents(instance);
      });
    }

    private static void BindGameInstanceComponents(GameInstance instance)
    {
      RootContainer.Bind<ICoroutineRunner>()
        .FromInstance(instance)
        .AsSingle();
    }

    #endregion

    #region Bindings

    public override void InstallBindings(ContainerBuilder builder)
    {
      // Logging
      BindLogging(builder);

      // Game State Machine
      BindGSM(builder);

      // Asset Management
      BindAssetManagement(builder);
      BindSceneLoading(builder);

      // Gameplay Services
      BindGameplayServices(builder);

      // Unity Services
      BindUnityServices(builder);
    }

    #endregion

    public override void LaunchGame() => _gameInstance.LaunchGame();

    #region Logging

    private void BindLogging(ContainerBuilder builder) =>
      builder.Bind<IGameLog>().To<GameLogger>().AsSingle();

    #endregion

    #region Game State Machine

    private void BindGSM(ContainerBuilder builder)
    {
      // StateFactory resolves states from container
      builder.Bind<StateFactory>().AsSingle();

      builder.Bind<IGameStateMachine>().To<GameStateMachine>().AsSingle();

      // States — AsTransient: fresh instance on each transition
      builder.Bind<BootstrapState>().AsTransient();
      builder.Bind<PreloadAssetsState>().AsTransient();
      builder.Bind<GameLoopState>().AsTransient();
    }

    #endregion

    #region Asset Management

    private void BindAssetManagement(ContainerBuilder builder)
    {
      builder.Bind<IAssetLoader>().To<AssetLoader>().AsSingle();
      builder.Bind<IAssetsPreloader>().To<AddressableAssetPreloader>().AsSingle();
    }

    private void BindSceneLoading(ContainerBuilder builder) =>
      builder.Bind<ISceneLoader>().To<AddressableSceneLoader>().AsSingle();

    #endregion

    #region Gameplay Services

    private void BindGameplayServices(ContainerBuilder builder)
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

    #endregion

    #region Unity Services

    private void BindUnityServices(ContainerBuilder builder) =>
      builder.Bind<ITimeService>().To<UnityTimeService>().AsSingle();

    #endregion
  }
}
