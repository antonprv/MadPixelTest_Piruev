// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

using Code.Data.StaticData;

using UnityEngine;

namespace Code.Model.Core
{
  /// <summary>
  /// Pure grid logic without MonoBehaviour.
  /// Knows nothing about Unity UI, knows nothing about DI.
  ///
  /// Invariants:
  ///  - _occupiedCells[cell] → item occupying this cell
  ///  - _items — list of all placed items (no duplicates)
  ///  - _activeCells — set of cells that are part of the bag
  /// </summary>
  public class GridInventory
  {
    private readonly HashSet<Vector2Int> _activeCells;
    private readonly Dictionary<Vector2Int, InventoryItem> _occupiedCells;
    private readonly List<InventoryItem> _items;

    public IReadOnlyList<InventoryItem> Items => _items;

    public GridInventory(HashSet<Vector2Int> activeCells)
    {
      _activeCells = new HashSet<Vector2Int>(activeCells);
      _occupiedCells = new Dictionary<Vector2Int, InventoryItem>();
      _items = new List<InventoryItem>();
    }

    #region Placement

    /// <summary>
    /// Checks if an item with config can be placed at origin.
    /// ignoredItem — item currently being dragged and already removed from grid
    /// (needed for highlight: so its cells are not considered occupied).
    /// </summary>
    public bool CanPlace(ItemConfig config, Vector2Int origin, InventoryItem ignoredItem = null)
    {
      foreach (var cell in config.GetOccupiedCells(origin))
      {
        if (!_activeCells.Contains(cell))
          return false;

        if (_occupiedCells.TryGetValue(cell, out var existing) && existing != ignoredItem)
          return false;
      }
      return true;
    }

    /// <summary>Places an item. Returns false if impossible.</summary>
    public bool TryPlace(InventoryItem item)
    {
      if (!CanPlace(item.Config, item.Origin))
        return false;

      foreach (var cell in item.GetOccupiedCells())
        _occupiedCells[cell] = item;

      _items.Add(item);
      return true;
    }

    /// <summary>Removes an item from the grid. Returns false if item not found.</summary>
    public bool TryRemove(InventoryItem item)
    {
      if (!_items.Remove(item))
        return false;

      foreach (var cell in item.GetOccupiedCells())
        _occupiedCells.Remove(cell);

      return true;
    }

    #endregion

    #region Query

    public InventoryItem GetItemAt(Vector2Int cell)
    {
      _occupiedCells.TryGetValue(cell, out var item);
      return item;
    }

    public bool IsCellActive(Vector2Int cell) => _activeCells.Contains(cell);
    public bool IsCellOccupied(Vector2Int cell) => _occupiedCells.ContainsKey(cell);

    public IReadOnlyCollection<Vector2Int> ActiveCells => _activeCells;

    #endregion

    #region Merge

    /// <summary>
    /// Checks if dragged can be merged with item at targetOrigin.
    /// Condition: both have same config, both support merge.
    /// </summary>
    public bool CanMerge(InventoryItem dragged, Vector2Int targetCell, out InventoryItem targetItem)
    {
      targetItem = GetItemAt(targetCell);

      if (targetItem == null || targetItem == dragged)
        return false;

      if (!dragged.Config.CanMerge)
        return false;

      if (dragged.Config != targetItem.Config)
        return false;

      // Check if result fits on targetItem's place
      var resultConfig = dragged.Config.MergeResult;
      return CanPlace(resultConfig, targetItem.Origin, targetItem);
    }

    /// <summary>
    /// Performs merge: removes both items, creates and places result.
    /// Returns new item.
    /// </summary>
    public InventoryItem Merge(InventoryItem a, InventoryItem b)
    {
      var resultOrigin = b.Origin; // result takes b's place (target)
      TryRemove(a);
      TryRemove(b);

      var merged = new InventoryItem(a.Config.MergeResult, resultOrigin);
      TryPlace(merged);
      return merged;
    }

    #endregion

    #region Config hot-swap

    /// <summary>
    /// Updates bag shape at runtime.
    /// Items that end up outside new active cells are returned in evicted list.
    /// </summary>
    public List<InventoryItem> UpdateActiveCells(HashSet<Vector2Int> newActiveCells)
    {
      var evicted = new List<InventoryItem>();

      foreach (var item in _items)
      {
        foreach (var cell in item.GetOccupiedCells())
        {
          if (!newActiveCells.Contains(cell))
          {
            evicted.Add(item);
            break;
          }
        }
      }

      foreach (var item in evicted)
        TryRemove(item);

      _activeCells.Clear();
      foreach (var cell in newActiveCells)
        _activeCells.Add(cell);

      return evicted;
    }

    #endregion
  }
}
