// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Core;
using Code.Infrastructure.StateMachine.States.Interfaces;
using Code.Services.Interfaces;

using Cysharp.Threading.Tasks;

using R3;

using UnityEngine;

namespace Code.Infrastructure.StateMachine
{
  /// <summary>
  /// State 3 of 3 — active gameplay.
  ///
  /// Responsibilities:
  ///   1. R3 subscriptions to inventory events (log, extensible hooks)
  ///   2. Wait for game exit (can be extended: pause, restart)
  ///
  /// All subscriptions live in _disposables and are automatically disposed in Exit().
  /// This guarantees no leaks on state transitions.
  ///
  /// Why R3 instead of events:
  ///   - Compose: can add Throttle / Debounce / Where without modifying services
  ///   - Dispose via CompositeDisposable — one line in Exit()
  ///   - Easy to test: substitute Subject in tests
  /// </summary>
  public class GameLoopState : IGameState
  {
    public StateType Type => StateType.GameLoop;

    private readonly IGridInventoryService _inventoryService;
    private readonly IBottomSlotsService _slotsService;

    private CompositeDisposable _disposables;

    public GameLoopState(
      IGridInventoryService inventoryService,
      IBottomSlotsService slotsService)
    {
      _inventoryService = inventoryService;
      _slotsService = slotsService;
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
    // Kept as virtual extension points:
    // in a real game these would be quest triggers, analytics, sound, etc.

    private void OnItemPlaced(InventoryItem item) =>
      Debug.Log($"[GameLoop] Placed: {item.Config.ItemId} at {item.Origin}");

    private void OnItemRemoved(InventoryItem item) =>
      Debug.Log($"[GameLoop] Removed: {item.Config.ItemId}");

    private void OnItemsMerged(MergeResult result) =>
      Debug.Log($"[GameLoop] Merged → {result.Result.Config.ItemId} (Lv{result.Result.Config.Level})");
  }
}
