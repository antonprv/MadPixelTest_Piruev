using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using BagFight.Core;
using BagFight.Services.Interfaces;
using BagFight.Infrastructure.StateMachine.States.Interfaces;

namespace BagFight.Infrastructure.StateMachine
{
  /// <summary>
  /// Стейт 3 из 3 — активный геймплей.
  ///
  /// Ответственность:
  ///   1. R3-подписки на события инвентаря (лог, расширяемые хуки)
  ///   2. Ожидание выхода из игры (можно расширить: пауза, рестарт)
  ///
  /// Все подписки живут в _disposables и автоматически снимаются в Exit().
  /// Это гарантирует отсутствие утечек при переходе между стейтами.
  ///
  /// Почему R3, а не events:
  ///   - Compose: можно добавить Throttle / Debounce / Where без изменения сервисов
  ///   - Dispose через CompositeDisposable — одна строка в Exit()
  ///   - Легко тестировать: подменяем Subject в тестах
  /// </summary>
  public class GameLoopState : IGameState
  {
    public StateType Type => StateType.GameLoop;

    private readonly IGridInventoryService _inventoryService;
    private readonly IBottomSlotsService   _slotsService;

    private CompositeDisposable _disposables;

    public GameLoopState(
      IGridInventoryService inventoryService,
      IBottomSlotsService   slotsService)
    {
      _inventoryService = inventoryService;
      _slotsService     = slotsService;
    }

    public void Enter()
    {
      _disposables = new CompositeDisposable();
      SubscribeToInventory();
    }

    public void Exit()
    {
      _disposables?.Dispose();
      _disposables = null;
    }

    // ─── R3 subscriptions ─────────────────────────────────────────────────────

    private void SubscribeToInventory()
    {
      // Лог каждого размещения — легко заменить на Achievement-триггер, аналитику и т.д.
      _inventoryService.OnItemPlaced
        .Subscribe(item => OnItemPlaced(item))
        .AddTo(_disposables);

      _inventoryService.OnItemRemoved
        .Subscribe(item => OnItemRemoved(item))
        .AddTo(_disposables);

      _inventoryService.OnItemsMerged
        .Subscribe(result => OnItemsMerged(result))
        .AddTo(_disposables);
    }

    // ─── Event handlers ───────────────────────────────────────────────────────
    // Оставлены как виртуальные точки расширения:
    // в реальной игре здесь были бы триггеры квестов, аналитика, звук и т.п.

    private void OnItemPlaced(InventoryItem item) =>
      Debug.Log($"[GameLoop] Placed: {item.Config.ItemId} at {item.Origin}");

    private void OnItemRemoved(InventoryItem item) =>
      Debug.Log($"[GameLoop] Removed: {item.Config.ItemId}");

    private void OnItemsMerged(MergeResult result) =>
      Debug.Log($"[GameLoop] Merged → {result.Result.Config.ItemId} (Lv{result.Result.Config.Level})");
  }
}
