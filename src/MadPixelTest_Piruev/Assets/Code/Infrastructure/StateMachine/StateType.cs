namespace BagFight.Infrastructure.StateMachine
{
  public enum StateType
  {
    None         = 0,
    Bootstrap    = 1,
    PreloadAssets = 2,
    GameLoop     = 3,
  }
}
