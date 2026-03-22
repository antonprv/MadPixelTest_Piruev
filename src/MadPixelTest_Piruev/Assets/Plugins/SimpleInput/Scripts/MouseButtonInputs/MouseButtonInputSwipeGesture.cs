// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

namespace SimpleInputNamespace
{
  public class MouseButtonInputSwipeGesture : SwipeGestureBase<int, bool>
  {
    public SimpleInput.MouseButtonInput mouseButton = new SimpleInput.MouseButtonInput();

    protected override BaseInput<int, bool> Input { get { return mouseButton; } }
    protected override bool Value { get { return true; } }

    public override int Priority { get { return 1; } }
  }
}