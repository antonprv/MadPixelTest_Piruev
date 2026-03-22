// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System;

using Code.Common.Extensions.Async;
using Code.Infrastructure.StateMachine;
using Code.Infrastructure.StateMachine.States;

using UnityEngine;

using Zenjex.Extensions.Attribute;
using Zenjex.Extensions.Injector;

namespace Code.Infrastructure.Installer
{
  [DefaultExecutionOrder(-10)]
  public class GameInstance : ZenjexBehaviour, ICoroutineRunner
  {
    public static GameInstance Instance { get; private set; }

    [Zenjex] private readonly IGameStateMachine _stateMachine;

    protected override void OnAwake()
    {
      base.OnAwake();
      RegisterSingletone();
      SetupFramerate();
    }

    private void SetupFramerate() =>
      Application.targetFrameRate = 120;

    public void LaunchGame() => StartGame();

    private void StartGame() =>
      _stateMachine.Enter<BootstrapState>();

    private void RegisterSingletone()
    {
      DontDestroyOnLoad(this);
      if (Instance != null && Instance != this)
      {
        Destroy(gameObject);
        return;
      }

      Instance = this;
    }
  }
}
