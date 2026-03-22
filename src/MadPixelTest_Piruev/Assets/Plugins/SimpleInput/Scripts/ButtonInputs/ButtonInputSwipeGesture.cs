// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

namespace SimpleInputNamespace
{
  public class ButtonInputSwipeGesture : SwipeGestureBase<string, bool>
  {
    public SimpleInput.ButtonInput button = new SimpleInput.ButtonInput();

    protected override BaseInput<string, bool> Input { get { return button; } }
    protected override bool Value { get { return true; } }

    public override int Priority { get { return 1; } }
  }
}