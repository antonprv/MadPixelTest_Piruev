using System.Collections;
using Reflex.Core;
using UnityEngine;
using BagFight.Data;
using BagFight.Services;
using BagFight.Services.Interfaces;
using BagFight.UI;
using Zenjex.Extensions.Core;

namespace BagFight.Infrastructure
{
  /// <summary>
  /// Корневой инсталлер проекта (Zenjex / Reflex).
  ///
  /// Привязки:
  ///   BagConfig             — экземпляр SO, серилизован в инспекторе
  ///   DragIconView          — сцена-объект, серилизован в инспекторе
  ///   GridInventoryService  — AsSingle + IInitializable (вызов Initialize() автоматически)
  ///   BottomSlotsService    — AsSingle + IInitializable
  ///   GridDragDropService   — AsSingle
  ///
  /// Порядок жизненного цикла Zenjex:
  ///   1. InstallBindings → ContainerReady → [Zenjex]-инъекция в сцене
  ///   2. InstallGameInstanceRoutine (null)
  ///   3. IInitializable.Initialize() на сервисах
  ///   4. LaunchGame (пусто — запуск происходит через Initialize)
  ///   5. OnGameLaunched → вторая волна инъекций (для динамически созданных объектов)
  /// </summary>
  public class BagInstaller : ProjectRootInstaller
  {
    [Header("Configs")]
    [SerializeField] private BagConfig _bagConfig;

    [Header("Scene references")]
    [SerializeField] private DragIconView _dragIconView;

    public override void InstallBindings(ContainerBuilder builder)
    {
      // ── Data ─────────────────────────────────────────────────────────────
      // BindInstance<T> сразу регистрирует как синглтон под контрактом T.
      builder.BindInstance(_bagConfig).AsSingle();
      builder.BindInstance(_dragIconView).AsSingle();

      // ── Services ─────────────────────────────────────────────────────────
      // Bind<ConcreteType>().BindInterfacesAndSelf() раскрывает все интерфейсы
      // конкретного класса как контракты, включая IInitializable —
      // тогда ProjectRootInstaller автоматически вызовет Initialize().

      builder.Bind<GridInventoryService>()
        .BindInterfacesAndSelf()   // → IGridInventoryService + IInitializable + сам тип
        .AsSingle();

      builder.Bind<BottomSlotsService>()
        .BindInterfacesAndSelf()   // → IBottomSlotsService + IInitializable + сам тип
        .AsSingle();

      // GridDragDropService не реализует IInitializable, просто синглтон
      builder.Bind<GridDragDropService>()
        .BindInterfacesAndSelf()   // → IGridDragDropService + сам тип
        .AsSingle();
    }

    public override IEnumerator InstallGameInstanceRoutine() => null;

    public override void LaunchGame() { /* GSM не нужен для тестового задания */ }
  }
}
