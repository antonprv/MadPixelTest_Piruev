// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

namespace Code.Model.Services.Startup
{
  /// <summary>
  /// Places a set of starter items into the inventory when the game session begins.
  /// Used for development testing and as a base for save/load initial state.
  /// </summary>
  public interface IStartupItemsService
  {
    /// <summary>
    /// Places startup items into the grid and bottom slots.
    /// Safe to call only after GridInventoryService and BottomSlotsService
    /// have been initialized.
    /// </summary>
    void PlaceStartupItems();
  }
}
