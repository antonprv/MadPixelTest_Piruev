// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Common.Extensions.Logging;
using Code.Infrastructure.StateMachine.States.Interfaces;
using Code.Infrastructure.StateMachine.States.Types;
using Code.Model.Core;
using Code.Model.Services.Startup;
using Code.Presenter.Bag;
using Code.UI.Factory;
using Code.UI.Hud;

using R3;

namespace Code.Infrastructure.StateMachine.States
{
  /// <summary>
  /// State 5 of 5 — active gameplay.
  ///
  /// Receives HudView as payload from LoadLevelState.
  /// Subscribes to HudView.OnReturnClicked → calls UIFactory.Cleanup()
  /// and transitions to MainMenuState.
  /// </summary>
  public class GameLoopState : IGamePayloadedState<HudView>
  {
    public StateType Type => StateType.GameLoop;

    private readonly IGameStateMachine   _gsm;
    private readonly IBagPresenter       _bagPresenter;
    private readonly IStartupItemsService _startupItems;
    private readonly IUIFactory          _uiFactory;
    private readonly IGameLog            _logger;

    private CompositeDisposable _disposables;
    private HudView              _hudView;

    public GameLoopState(
      IGameStateMachine    gsm,
      IBagPresenter        bagPresenter,
      IStartupItemsService startupItems,
      IUIFactory           uiFactory,
      IGameLog             logger)
    {
      _gsm          = gsm;
      _bagPresenter = bagPresenter;
      _startupItems = startupItems;
      _uiFactory    = uiFactory;
      _logger       = logger;
    }

    public void Enter(HudView hudView)
    {
      _hudView     = hudView;
      _disposables = new CompositeDisposable();

      _startupItems.PlaceStartupItems();
      SubscribeToInventory();

      if (_hudView != null)
        _hudView.OnReturnClicked += OnReturnToMenuClicked;
      else
        _logger.Log("HudView is null — return button unavailable.");
    }

    public void Exit()
    {
      if (_hudView != null)
        _hudView.OnReturnClicked -= OnReturnToMenuClicked;

      _disposables?.Dispose();
      _disposables = null;
      _hudView     = null;
    }

    // ── Return to menu ─────────────────────────────────────────────────────

    private void OnReturnToMenuClicked()
    {
      // Cleanup destroys UI_Root + all children (BagCanvas, HUD)
      _uiFactory.Cleanup();
      _gsm.Enter<MainMenuState>();
    }

    // ── Inventory subscriptions ────────────────────────────────────────────

    private void SubscribeToInventory()
    {
      _bagPresenter.OnItemPlaced
        .Subscribe(item => _logger.Log($"[GameLoop] Placed: {item.Config.ItemId} at {item.Origin}"))
        .AddTo(_disposables);

      _bagPresenter.OnItemRemoved
        .Subscribe(item => _logger.Log($"[GameLoop] Removed: {item.Config.ItemId}"))
        .AddTo(_disposables);

      _bagPresenter.OnItemsMerged
        .Subscribe(r => _logger.Log($"[GameLoop] Merged → {r.Result.Config.ItemId} (Lv{r.Result.Config.Level})"))
        .AddTo(_disposables);
    }
  }
}
