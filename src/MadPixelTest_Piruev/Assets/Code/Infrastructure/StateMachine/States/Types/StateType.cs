// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

namespace Code.Infrastructure.StateMachine.States.Types
{
  public enum StateType
  {
    None = 0,
    Bootstrap = 1,
    PreloadAssets = 2,
    LoadLevel = 3,
    GameLoop = 4,
  }
}
