// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

namespace Code.Infrastructure.StateMachine
{
  public enum StateType
  {
    None = 0,
    Bootstrap = 1,
    PreloadAssets = 2,
    GameLoop = 3,
  }
}
