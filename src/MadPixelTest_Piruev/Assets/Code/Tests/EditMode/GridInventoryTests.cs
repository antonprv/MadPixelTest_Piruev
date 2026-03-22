// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;
using System.Linq;

using Code.Core;

using NUnit.Framework;

using UnityEngine;

namespace Code.Tests.EditMode
{
  /// <summary>
  /// EditMode tests for pure GridInventory logic.
  /// Run: Window → General → Test Runner → EditMode.
  /// </summary>
  [TestFixture]
  public class GridInventoryTests
  {
    private GridInventory _grid;

    [SetUp]
    public void SetUp()
    {
      _grid = InventoryTestHelpers.MakeGrid(5, 7);
    }

    #region Placement — basic cases

    [Test]
    public void CanPlace_SingleCell_InBounds_ReturnsTrue()
    {
      var cfg = InventoryTestHelpers.Single();
      Assert.IsTrue(_grid.CanPlace(cfg, new Vector2Int(2, 3)));
    }

    [Test]
    public void CanPlace_SingleCell_OutOfBounds_ReturnsFalse()
    {
      var cfg = InventoryTestHelpers.Single();
      Assert.IsFalse(_grid.CanPlace(cfg, new Vector2Int(99, 99)));
    }

    [Test]
    public void TryPlace_Single_Succeeds_AndOccupiesCell()
    {
      var cfg = InventoryTestHelpers.Single();
      var item = new InventoryItem(cfg, new Vector2Int(1, 1));

      Assert.IsTrue(_grid.TryPlace(item));
      Assert.AreSame(item, _grid.GetItemAt(new Vector2Int(1, 1)));
    }

    [Test]
    public void TryPlace_LShape_OccupiesAllThreeCells()
    {
      var cfg = InventoryTestHelpers.LShape();
      var item = new InventoryItem(cfg, Vector2Int.zero);

      Assert.IsTrue(_grid.TryPlace(item));

      Assert.AreSame(item, _grid.GetItemAt(new Vector2Int(0, 0)));
      Assert.AreSame(item, _grid.GetItemAt(new Vector2Int(0, 1)));
      Assert.AreSame(item, _grid.GetItemAt(new Vector2Int(1, 1)));
    }

    [Test]
    public void TryPlace_SamePositionTwice_SecondFails()
    {
      var cfg = InventoryTestHelpers.Single();
      var item1 = new InventoryItem(cfg, new Vector2Int(0, 0));
      var item2 = new InventoryItem(cfg, new Vector2Int(0, 0));

      _grid.TryPlace(item1);
      Assert.IsFalse(_grid.TryPlace(item2));
    }

    [Test]
    public void TryPlace_PartiallyOutOfBounds_Fails()
    {
      // Horizontal2 at position (4,0) → cell (5,0) is outside x=4 (max)
      var cfg = InventoryTestHelpers.Horizontal2();
      var item = new InventoryItem(cfg, new Vector2Int(4, 0));

      Assert.IsFalse(_grid.TryPlace(item));
    }

    #endregion

    #region Remove

    [Test]
    public void TryRemove_PlacedItem_Succeeds_CellsBecomeEmpty()
    {
      var cfg = InventoryTestHelpers.LShape();
      var item = InventoryTestHelpers.PlaceItem(_grid, cfg, Vector2Int.zero);

      Assert.IsTrue(_grid.TryRemove(item));
      Assert.IsNull(_grid.GetItemAt(new Vector2Int(0, 0)));
      Assert.IsNull(_grid.GetItemAt(new Vector2Int(0, 1)));
      Assert.IsNull(_grid.GetItemAt(new Vector2Int(1, 1)));
    }

    [Test]
    public void TryRemove_NotPlacedItem_ReturnsFalse()
    {
      var cfg = InventoryTestHelpers.Single();
      var item = new InventoryItem(cfg, Vector2Int.zero); // not placed

      Assert.IsFalse(_grid.TryRemove(item));
    }

    [Test]
    public void AfterRemove_CellIsAvailableForNewItem()
    {
      var cfg = InventoryTestHelpers.Single();
      var item1 = InventoryTestHelpers.PlaceItem(_grid, cfg, new Vector2Int(2, 2));
      _grid.TryRemove(item1);

      var item2 = new InventoryItem(cfg, new Vector2Int(2, 2));
      Assert.IsTrue(_grid.TryPlace(item2));
    }

    #endregion

    #region GetItemAt / Items list

    [Test]
    public void GetItemAt_EmptyCell_ReturnsNull()
    {
      Assert.IsNull(_grid.GetItemAt(new Vector2Int(0, 0)));
    }

    [Test]
    public void Items_ReflectsPlacedAndRemovedItems()
    {
      var cfg = InventoryTestHelpers.Single();
      var item1 = InventoryTestHelpers.PlaceItem(_grid, cfg, new Vector2Int(0, 0));
      var item2 = InventoryTestHelpers.PlaceItem(_grid, cfg, new Vector2Int(1, 0));

      Assert.AreEqual(2, _grid.Items.Count);

      _grid.TryRemove(item1);
      Assert.AreEqual(1, _grid.Items.Count);
      Assert.IsTrue(_grid.Items.Contains(item2));
    }

    #endregion

    #region Merge

    [Test]
    public void CanMerge_SameConfigWithMergeResult_ReturnsTrue()
    {
      var resultCfg = InventoryTestHelpers.Single(level: 2);
      var cfg = InventoryTestHelpers.Single(level: 1, mergeResult: resultCfg);

      var item1 = InventoryTestHelpers.PlaceItem(_grid, cfg, new Vector2Int(0, 0));
      var item2 = InventoryTestHelpers.PlaceItem(_grid, cfg, new Vector2Int(1, 0));

      // Drag item2 onto cell (0,0) where item1 is located
      Assert.IsTrue(_grid.CanMerge(item2, new Vector2Int(0, 0), out var found));
      Assert.AreSame(item1, found);
    }

    [Test]
    public void CanMerge_DifferentConfigs_ReturnsFalse()
    {
      var cfgA = InventoryTestHelpers.Single(level: 1, mergeResult: InventoryTestHelpers.Single(2));
      var cfgB = InventoryTestHelpers.Vertical2(level: 1);

      var itemA = InventoryTestHelpers.PlaceItem(_grid, cfgA, new Vector2Int(0, 0));
      var itemB = new InventoryItem(cfgB, new Vector2Int(1, 0)); // not placed (dragging)

      Assert.IsFalse(_grid.CanMerge(itemB, new Vector2Int(0, 0), out _));
    }

    [Test]
    public void CanMerge_NullMergeResult_ReturnsFalse()
    {
      // MergeResult not set → CanMerge should return false
      var cfg = InventoryTestHelpers.Single(level: 1, mergeResult: null);
      var item1 = InventoryTestHelpers.PlaceItem(_grid, cfg, new Vector2Int(0, 0));
      var item2 = new InventoryItem(cfg, new Vector2Int(1, 0));

      Assert.IsFalse(_grid.CanMerge(item2, new Vector2Int(0, 0), out _));
    }

    [Test]
    public void Merge_ProducesCorrectResultItem()
    {
      var resultCfg = InventoryTestHelpers.Single(level: 2);
      var cfg = InventoryTestHelpers.Single(level: 1, mergeResult: resultCfg);

      var item1 = InventoryTestHelpers.PlaceItem(_grid, cfg, new Vector2Int(0, 0));
      var item2 = InventoryTestHelpers.PlaceItem(_grid, cfg, new Vector2Int(1, 0));

      var merged = _grid.Merge(item2, item1);

      Assert.AreSame(resultCfg, merged.Config);
      Assert.AreEqual(new Vector2Int(0, 0), merged.Origin); // takes target's place
    }

    [Test]
    public void Merge_OriginalItemsRemovedFromGrid()
    {
      var resultCfg = InventoryTestHelpers.Single(level: 2);
      var cfg = InventoryTestHelpers.Single(level: 1, mergeResult: resultCfg);

      var item1 = InventoryTestHelpers.PlaceItem(_grid, cfg, new Vector2Int(0, 0));
      var item2 = InventoryTestHelpers.PlaceItem(_grid, cfg, new Vector2Int(1, 0));

      _grid.Merge(item2, item1);

      Assert.IsFalse(_grid.Items.Contains(item1));
      Assert.IsFalse(_grid.Items.Contains(item2));
    }

    [Test]
    public void Merge_ResultIsInItemsList()
    {
      var resultCfg = InventoryTestHelpers.Single(level: 2);
      var cfg = InventoryTestHelpers.Single(level: 1, mergeResult: resultCfg);

      var item1 = InventoryTestHelpers.PlaceItem(_grid, cfg, new Vector2Int(0, 0));
      var item2 = InventoryTestHelpers.PlaceItem(_grid, cfg, new Vector2Int(1, 0));

      var merged = _grid.Merge(item2, item1);

      Assert.AreEqual(1, _grid.Items.Count);
      Assert.IsTrue(_grid.Items.Contains(merged));
    }

    #endregion

    #region UpdateActiveCells (bag shape changes at runtime)

    [Test]
    public void UpdateActiveCells_ItemInRemovedCell_IsEvicted()
    {
      var cfg = InventoryTestHelpers.Single();
      var item = InventoryTestHelpers.PlaceItem(_grid, cfg, new Vector2Int(4, 6));

      // Reduce grid to 4×6 — cell (4,6) is removed
      var newCells = new HashSet<Vector2Int>();
      for (int x = 0; x < 4; x++)
        for (int y = 0; y < 6; y++)
          newCells.Add(new Vector2Int(x, y));

      var evicted = _grid.UpdateActiveCells(newCells);

      Assert.AreEqual(1, evicted.Count);
      Assert.AreSame(item, evicted[0]);
      Assert.IsNull(_grid.GetItemAt(new Vector2Int(4, 6)));
    }

    [Test]
    public void UpdateActiveCells_ItemFullyInNewGrid_NotEvicted()
    {
      var cfg = InventoryTestHelpers.Single();
      var item = InventoryTestHelpers.PlaceItem(_grid, cfg, new Vector2Int(0, 0));

      // Expand to 6×8 — (0,0) remains active
      var newCells = new HashSet<Vector2Int>();
      for (int x = 0; x < 6; x++)
        for (int y = 0; y < 8; y++)
          newCells.Add(new Vector2Int(x, y));

      var evicted = _grid.UpdateActiveCells(newCells);

      Assert.AreEqual(0, evicted.Count);
      Assert.AreSame(item, _grid.GetItemAt(new Vector2Int(0, 0)));
    }

    [Test]
    public void UpdateActiveCells_LShapePartiallyOutside_WholeItemEvicted()
    {
      // L-shape: (0,0), (0,1), (1,1) — placed in corner
      var cfg = InventoryTestHelpers.LShape();
      var item = InventoryTestHelpers.PlaceItem(_grid, cfg, Vector2Int.zero);

      // New grid without column x=1 — cell (1,1) is missing → item should be fully evicted
      var newCells = new HashSet<Vector2Int>();
      for (int x = 0; x < 1; x++)
        for (int y = 0; y < 7; y++)
          newCells.Add(new Vector2Int(x, y));

      var evicted = _grid.UpdateActiveCells(newCells);

      Assert.AreEqual(1, evicted.Count);
      Assert.AreSame(item, evicted[0]);
    }

    #endregion

    #region IsCellActive / IsCellOccupied

    [Test]
    public void IsCellActive_WithinBounds_IsTrue()
    {
      Assert.IsTrue(_grid.IsCellActive(new Vector2Int(0, 0)));
      Assert.IsTrue(_grid.IsCellActive(new Vector2Int(4, 6)));
    }

    [Test]
    public void IsCellActive_OutsideBounds_IsFalse()
    {
      Assert.IsFalse(_grid.IsCellActive(new Vector2Int(5, 0)));
      Assert.IsFalse(_grid.IsCellActive(new Vector2Int(-1, 0)));
    }

    [Test]
    public void IsCellOccupied_AfterPlace_IsTrue()
    {
      var item = InventoryTestHelpers.PlaceItem(_grid, InventoryTestHelpers.Single(), new Vector2Int(2, 2));
      Assert.IsTrue(_grid.IsCellOccupied(new Vector2Int(2, 2)));
    }

    [Test]
    public void IsCellOccupied_AfterRemove_IsFalse()
    {
      var item = InventoryTestHelpers.PlaceItem(_grid, InventoryTestHelpers.Single(), new Vector2Int(2, 2));
      _grid.TryRemove(item);
      Assert.IsFalse(_grid.IsCellOccupied(new Vector2Int(2, 2)));
    }

    #endregion

    #region Edge cases

    [Test]
    public void CanPlace_WithIgnoredItem_IgnoredCellsAreFree()
    {
      // Situation: dragging an item, it's already removed from grid (ignored).
      // Trying to place it on adjacent position — should not conflict.
      var cfg = InventoryTestHelpers.Horizontal2();
      var item = new InventoryItem(cfg, new Vector2Int(0, 0));
      _grid.TryPlace(item);
      _grid.TryRemove(item); // remove (simulate drag start)

      // Now try to place with ignored=item — item's cells are considered free
      Assert.IsTrue(_grid.CanPlace(cfg, new Vector2Int(1, 0), item));
    }

    [Test]
    public void PlaceMultipleItems_NoOverlap_AllSucceed()
    {
      var cfg = InventoryTestHelpers.Single();

      for (int x = 0; x < 5; x++)
      {
        var item = new InventoryItem(cfg, new Vector2Int(x, 0));
        Assert.IsTrue(_grid.TryPlace(item), $"Failed to place at ({x}, 0)");
      }

      Assert.AreEqual(5, _grid.Items.Count);
    }

    [Test]
    public void Square2x2_PlacedAtCorner_OccupiesFourCells()
    {
      var cfg = InventoryTestHelpers.Square2x2();
      var item = InventoryTestHelpers.PlaceItem(_grid, cfg, new Vector2Int(0, 0));

      Assert.AreSame(item, _grid.GetItemAt(new Vector2Int(0, 0)));
      Assert.AreSame(item, _grid.GetItemAt(new Vector2Int(1, 0)));
      Assert.AreSame(item, _grid.GetItemAt(new Vector2Int(0, 1)));
      Assert.AreSame(item, _grid.GetItemAt(new Vector2Int(1, 1)));
    }

    #endregion
  }
}
