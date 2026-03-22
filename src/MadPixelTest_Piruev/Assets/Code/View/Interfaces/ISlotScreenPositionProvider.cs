// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using UnityEngine;

namespace Code.View.Interfaces
{
  /// <summary>
  /// Provides screen-space center positions of bottom hotbar slots.
  /// Implemented by BottomSlotsView and injected into DragDropPresenter
  /// so it can tell DragIconViewModel where to fly the icon on cancel/return.
  /// </summary>
  public interface ISlotScreenPositionProvider
  {
    /// <summary>
    /// Returns the screen-space center of the slot at <paramref name="slotIndex"/>.
    /// Returns Vector2.zero if the index is out of range.
    /// </summary>
    Vector2 GetSlotScreenPosition(int slotIndex);
  }
}
