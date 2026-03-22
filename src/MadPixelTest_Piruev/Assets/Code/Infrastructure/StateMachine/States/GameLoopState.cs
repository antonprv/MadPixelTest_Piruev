// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Common.Extensions.Logging;
using Code.Infrastructure.StateMachine.States.Interfaces;
using Code.Infrastructure.StateMachine.States.Types;
using Code.Model.Core;
using Code.Model.Services.Inventory.Interfaces;
using Code.Model.Services.Startup;
using Code.Presenter.Bag;

using R3;

namespace Code.Infrastructure.StateMachine.States
{
  /// <summary>
  /// State 4 of 4 — active gameplay.
  ///
  /// On Enter:
  ///   1. IStartupItemsService.PlaceStartupItems() — fills inventory with
  ///      starter items from the manifest. Safe to call here because
  ///      LoadLevelState already called InitializeModelServices() before
  ///      transitioning to this state.
  ///   2. Subscribes to IBagPresenter events for game-level reactions
  ///      (analytics, sound, quests — currently just logging).
  /// </summary>
  public class GameLoopState : IGameState
  {
    public StateType Type => StateType.GameLoop;

    private readonly IBagPresenter        _bagPresenter;
    private readonly IStartupItemsService _startupItems;
    private readonly IGameLog             _logger;

    private CompositeDisposable _disposables;

    public GameLoopState(
      IBagPresenter        bagPresenter,
      IStartupItemsService startupItems,
      IGameLog             logger)
    {
      _bagPresenter = bagPresenter;
      _startupItems = startupItems;
      _logger       = logger;
    }

    public void Enter()
    {
      _disposables = new CompositeDisposable();

      _startupItems.PlaceStartupItems();

      SubscribeToInventory();
    }

    public void Exit()
    {
      _disposables?.Dispose();
      _disposables = null;
    }

    #region R3 subscriptions

    private void SubscribeToInventory()
    {
      _bagPresenter.OnItemPlaced
        .Subscribe(OnItemPlaced)
        .AddTo(_disposables);

      _bagPresenter.OnItemRemoved
        .Subscribe(OnItemRemoved)
        .AddTo(_disposables);

      _bagPresenter.OnItemsMerged
        .Subscribe(OnItemsMerged)
        .AddTo(_disposables);
    }

    #endregion

    #region Handlers (extension points)

    private void OnItemPlaced(InventoryItem item) =>
      _logger.Log($"[GameLoop] Placed: {item.Config.ItemId} at {item.Origin}");

    private void OnItemRemoved(InventoryItem item) =>
      _logger.Log($"[GameLoop] Removed: {item.Config.ItemId}");

    private void OnItemsMerged(MergeResult result) =>
      _logger.Log($"[GameLoop] Merged → {result.Result.Config.ItemId} (Lv{result.Result.Config.Level})");

    #endregion
  }
}
