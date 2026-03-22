using System.Collections.Generic;
using UnityEngine;
using BagFight.Core;
using BagFight.Data;

namespace BagFight.Tests
{
  /// <summary>
  /// Вспомогательные методы для создания тестовых объектов без Unity-контекста.
  /// ItemConfig создаётся через ScriptableObject.CreateInstance — работает в EditMode.
  /// </summary>
  internal static class InventoryTestHelpers
  {
    // ─── ItemConfig builders ─────────────────────────────────────────────────

    /// <summary>1×1 предмет (одна клетка)</summary>
    public static ItemConfig Single(int level = 1, ItemConfig mergeResult = null)
    {
      var cfg = ScriptableObject.CreateInstance<ItemConfig>();
      cfg.SetTestData("single", level, new List<Vector2Int> { Vector2Int.zero }, mergeResult);
      return cfg;
    }

    /// <summary>1×2 предмет (вертикальная полоска)</summary>
    public static ItemConfig Vertical2(int level = 1)
    {
      var cfg = ScriptableObject.CreateInstance<ItemConfig>();
      cfg.SetTestData("v2", level, new List<Vector2Int>
      {
        new(0, 0), new(0, 1)
      });
      return cfg;
    }

    /// <summary>2×1 предмет (горизонтальная полоска)</summary>
    public static ItemConfig Horizontal2(int level = 1)
    {
      var cfg = ScriptableObject.CreateInstance<ItemConfig>();
      cfg.SetTestData("h2", level, new List<Vector2Int>
      {
        new(0, 0), new(1, 0)
      });
      return cfg;
    }

    /// <summary>L-форма: (0,0),(0,1),(1,1)</summary>
    public static ItemConfig LShape(int level = 1)
    {
      var cfg = ScriptableObject.CreateInstance<ItemConfig>();
      cfg.SetTestData("lshape", level, new List<Vector2Int>
      {
        new(0, 0), new(0, 1), new(1, 1)
      });
      return cfg;
    }

    /// <summary>2×2 квадрат</summary>
    public static ItemConfig Square2x2(int level = 1)
    {
      var cfg = ScriptableObject.CreateInstance<ItemConfig>();
      cfg.SetTestData("sq2x2", level, new List<Vector2Int>
      {
        new(0, 0), new(1, 0), new(0, 1), new(1, 1)
      });
      return cfg;
    }

    // ─── GridInventory builder ────────────────────────────────────────────────

    /// <summary>Создаёт прямоугольный грид width×height.</summary>
    public static GridInventory MakeGrid(int width = 5, int height = 7)
    {
      var cells = new System.Collections.Generic.HashSet<Vector2Int>();
      for (int x = 0; x < width; x++)
      for (int y = 0; y < height; y++)
        cells.Add(new Vector2Int(x, y));
      return new GridInventory(cells);
    }

    /// <summary>Создаёт предмет и сразу размещает его на гриде.</summary>
    public static InventoryItem PlaceItem(GridInventory grid, ItemConfig config, Vector2Int origin)
    {
      var item = new InventoryItem(config, origin);
      grid.TryPlace(item);
      return item;
    }
  }

  /// <summary>
  /// Reflection-based extension чтобы задавать приватные поля ItemConfig в тестах.
  /// В продакшн-коде не используется.
  /// </summary>
  internal static class ItemConfigTestExtensions
  {
    public static void SetTestData(
      this ItemConfig cfg,
      string id,
      int level,
      List<Vector2Int> shape,
      ItemConfig mergeResult = null)
    {
      var t = typeof(ItemConfig);
      SetBacking(t, cfg, "ItemId",      id);
      SetBacking(t, cfg, "Level",       level);
      SetBacking(t, cfg, "Shape",       shape);
      SetBacking(t, cfg, "MergeResult", mergeResult);
    }

    private static void SetBacking(System.Type type, object obj, string propName, object value)
    {
      // Auto-property backing field: <PropName>k__BackingField
      var field = type.GetField(
        $"<{propName}>k__BackingField",
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
      field?.SetValue(obj, value);
    }
  }
}
