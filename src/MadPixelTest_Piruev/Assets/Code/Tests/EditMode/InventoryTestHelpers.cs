// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

using Code.Data.StaticData.Configs;

using Code.Model.Core;

using UnityEngine;

namespace Code.Tests.EditMode
{
  /// <summary>
  /// Helper methods for creating test objects without Unity context.
  /// ItemConfig is created via ScriptableObject.CreateInstance — works in EditMode.
  /// </summary>
  internal static class InventoryTestHelpers
  {
    #region ItemConfig builders

    /// <summary>1×1 item (single cell)</summary>
    public static ItemConfig Single(int level = 1, ItemConfig mergeResult = null)
    {
      var cfg = ScriptableObject.CreateInstance<ItemConfig>();
      cfg.SetTestData("single", level, new List<Vector2Int> { Vector2Int.zero }, mergeResult);
      return cfg;
    }

    /// <summary>1×2 item (vertical bar)</summary>
    public static ItemConfig Vertical2(int level = 1)
    {
      var cfg = ScriptableObject.CreateInstance<ItemConfig>();
      cfg.SetTestData("v2", level, new List<Vector2Int>
      {
        new(0, 0), new(0, 1)
      });
      return cfg;
    }

    /// <summary>2×1 item (horizontal bar)</summary>
    public static ItemConfig Horizontal2(int level = 1)
    {
      var cfg = ScriptableObject.CreateInstance<ItemConfig>();
      cfg.SetTestData("h2", level, new List<Vector2Int>
      {
        new(0, 0), new(1, 0)
      });
      return cfg;
    }

    /// <summary>L-shape: (0,0),(0,1),(1,1)</summary>
    public static ItemConfig LShape(int level = 1)
    {
      var cfg = ScriptableObject.CreateInstance<ItemConfig>();
      cfg.SetTestData("lshape", level, new List<Vector2Int>
      {
        new(0, 0), new(0, 1), new(1, 1)
      });
      return cfg;
    }

    /// <summary>2×2 square</summary>
    public static ItemConfig Square2x2(int level = 1)
    {
      var cfg = ScriptableObject.CreateInstance<ItemConfig>();
      cfg.SetTestData("sq2x2", level, new List<Vector2Int>
      {
        new(0, 0), new(1, 0), new(0, 1), new(1, 1)
      });
      return cfg;
    }

    #endregion

    #region GridInventory builder

    /// <summary>Creates a rectangular grid width×height.</summary>
    public static GridInventory MakeGrid(int width = 5, int height = 7)
    {
      var cells = new System.Collections.Generic.HashSet<Vector2Int>();
      for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
          cells.Add(new Vector2Int(x, y));
      return new GridInventory(cells);
    }

    /// <summary>Creates an item and immediately places it on the grid.</summary>
    public static InventoryItem PlaceItem(GridInventory grid, ItemConfig config, Vector2Int origin)
    {
      var item = new InventoryItem(config, origin);
      grid.TryPlace(item);
      return item;
    }

    #endregion
  }

  /// <summary>
  /// Reflection-based extension to set private fields of ItemConfig in tests.
  /// Not used in production code.
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
      SetBacking(t, cfg, "ItemId", id);
      SetBacking(t, cfg, "Level", level);
      SetBacking(t, cfg, "Shape", shape);
      SetBacking(t, cfg, "MergeResult", mergeResult);
      // Don't set Icon (AssetReferenceSprite) in tests — grid logic doesn't depend on it
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
