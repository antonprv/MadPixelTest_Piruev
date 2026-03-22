using System.Collections.Generic;
using UnityEngine;
using BagFight.Data;

namespace BagFight.Core
{
  /// <summary>
  /// Чистая логика грида без MonoBehaviour.
  /// Не знает о Unity UI, не знает о DI.
  ///
  /// Инварианты:
  ///  - _occupiedCells[cell] → предмет, занимающий эту клетку
  ///  - _items — список всех размещённых предметов (без дублей)
  ///  - _activeCells — множество клеток, которые являются частью сумки
  /// </summary>
  public class GridInventory
  {
    private readonly HashSet<Vector2Int>              _activeCells;
    private readonly Dictionary<Vector2Int, InventoryItem> _occupiedCells;
    private readonly List<InventoryItem>              _items;

    public IReadOnlyList<InventoryItem> Items => _items;

    public GridInventory(HashSet<Vector2Int> activeCells)
    {
      _activeCells   = new HashSet<Vector2Int>(activeCells);
      _occupiedCells = new Dictionary<Vector2Int, InventoryItem>();
      _items         = new List<InventoryItem>();
    }

    // ─── Placement ────────────────────────────────────────────────────────────

    /// <summary>
    /// Проверяет, можно ли разместить предмет с config по origin.
    /// ignoredItem — предмет, который сейчас тащится и уже убран с грида
    /// (нужен для highlight: чтобы не считать его клетки занятыми).
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

    /// <summary>Размещает предмет. Возвращает false если невозможно.</summary>
    public bool TryPlace(InventoryItem item)
    {
      if (!CanPlace(item.Config, item.Origin))
        return false;

      foreach (var cell in item.GetOccupiedCells())
        _occupiedCells[cell] = item;

      _items.Add(item);
      return true;
    }

    /// <summary>Убирает предмет с грида. Возвращает false если предмет не найден.</summary>
    public bool TryRemove(InventoryItem item)
    {
      if (!_items.Contains(item))
        return false;

      foreach (var cell in item.GetOccupiedCells())
        _occupiedCells.Remove(cell);

      _items.Remove(item);
      return true;
    }

    // ─── Query ────────────────────────────────────────────────────────────────

    public InventoryItem GetItemAt(Vector2Int cell)
    {
      _occupiedCells.TryGetValue(cell, out var item);
      return item;
    }

    public bool IsCellActive(Vector2Int cell)   => _activeCells.Contains(cell);
    public bool IsCellOccupied(Vector2Int cell) => _occupiedCells.ContainsKey(cell);

    public IReadOnlyCollection<Vector2Int> ActiveCells => _activeCells;

    // ─── Merge ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Проверяет, можно ли слить dragged с предметом в targetOrigin.
    /// Условие: оба одинакового конфига, оба поддерживают мерж.
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

      // Проверяем, влезет ли результат на место targetItem
      var resultConfig = dragged.Config.MergeResult;
      return CanPlace(resultConfig, targetItem.Origin, targetItem);
    }

    /// <summary>
    /// Выполняет мерж: убирает оба предмета, создаёт и размещает результат.
    /// Возвращает новый предмет.
    /// </summary>
    public InventoryItem Merge(InventoryItem a, InventoryItem b)
    {
      var resultOrigin = b.Origin; // результат встаёт на место b (цели)
      TryRemove(a);
      TryRemove(b);

      var merged = new InventoryItem(a.Config.MergeResult, resultOrigin);
      TryPlace(merged);
      return merged;
    }

    // ─── Config hot-swap ──────────────────────────────────────────────────────

    /// <summary>
    /// Обновляет форму сумки в рантайме.
    /// Предметы, оказавшиеся вне новых активных клеток, возвращаются в список evicted.
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
  }
}
