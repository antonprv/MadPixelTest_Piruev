// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

namespace Code.Infrastructure.StateMachine.States.Types
{
  public enum StateType
  {
    None        = 0,
    Bootstrap   = 1,
    PreloadAssets = 2,
    MainMenu    = 3,
    LoadLevel   = 4,
    GameLoop    = 5,
  }
}
