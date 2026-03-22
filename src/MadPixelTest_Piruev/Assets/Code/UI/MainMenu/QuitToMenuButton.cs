// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System;

using Code.Infrastructure.StateMachine;
using Code.Infrastructure.StateMachine.States;

using UnityEngine;
using UnityEngine.UI;

using Zenjex.Extensions.Attribute;
using Zenjex.Extensions.Injector;

namespace Code.UI.MainMenu
{
  public class QuitToMenuButton : ZenjexBehaviour
  {
    [SerializeField] private Button _quitButton;

    [Zenjex] private readonly IGameStateMachine _gsm;

    protected override void OnAwake()
    {
      base.OnAwake();

      if (_quitButton != null)
        _quitButton.onClick.AddListener(HandleQuit);
    }

    private void HandleQuit() => _gsm.Enter<MainMenuState>();

    private void OnDestroy()
    {
      if (_quitButton != null)
        _quitButton.onClick.RemoveAllListeners();
    }
  }
}
