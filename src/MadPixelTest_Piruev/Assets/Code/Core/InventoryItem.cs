// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

using Code.Data.StaticData;

using UnityEngine;

namespace Code.Core
{
  /// <summary>
  /// Runtime instance of an item on the grid.
  /// Stores config + current origin (top-left corner of bounding box).
  /// </summary>
  public class InventoryItem
  {
    public ItemConfig Config { get; }
    public Vector2Int Origin { get; private set; }

    public InventoryItem(ItemConfig config, Vector2Int origin)
    {
      Config = config;
      Origin = origin;
    }

    /// <summary>Updates position (used for returning to place after failed drop).</summary>
    public void SetOrigin(Vector2Int origin) => Origin = origin;

    /// <summary>Enumerates all cells occupied by the item on the grid.</summary>
    public IEnumerable<Vector2Int> GetOccupiedCells() => Config.GetOccupiedCells(Origin);
  }
}
