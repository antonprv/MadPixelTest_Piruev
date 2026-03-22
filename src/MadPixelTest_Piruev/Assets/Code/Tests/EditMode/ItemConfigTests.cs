// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System.Linq;

using NUnit.Framework;

using UnityEngine;

namespace Code.Tests.EditMode
{
  [TestFixture]
  public class ItemConfigTests
  {
    [Test]
    public void GetOccupiedCells_SingleCell_ReturnsOriginOnly()
    {
      var cfg = InventoryTestHelpers.Single();
      var cells = cfg.GetOccupiedCells(new Vector2Int(3, 4)).ToList();

      Assert.AreEqual(1, cells.Count);
      Assert.AreEqual(new Vector2Int(3, 4), cells[0]);
    }

    [Test]
    public void GetOccupiedCells_LShape_ReturnsCorrectThreeCells()
    {
      var cfg = InventoryTestHelpers.LShape();
      var origin = new Vector2Int(2, 2);
      var cells = cfg.GetOccupiedCells(origin).ToList();

      Assert.AreEqual(3, cells.Count);
      Assert.Contains(new Vector2Int(2, 2), cells);
      Assert.Contains(new Vector2Int(2, 3), cells);
      Assert.Contains(new Vector2Int(3, 3), cells);
    }

    [Test]
    public void GetBoundsSize_Single_Returns1x1()
    {
      var cfg = InventoryTestHelpers.Single();
      Assert.AreEqual(new Vector2Int(1, 1), cfg.GetBoundsSize());
    }

    [Test]
    public void GetBoundsSize_Horizontal2_Returns2x1()
    {
      var cfg = InventoryTestHelpers.Horizontal2();
      Assert.AreEqual(new Vector2Int(2, 1), cfg.GetBoundsSize());
    }

    [Test]
    public void GetBoundsSize_LShape_Returns2x2()
    {
      // L: (0,0),(0,1),(1,1) → bounding box 2×2
      var cfg = InventoryTestHelpers.LShape();
      Assert.AreEqual(new Vector2Int(2, 2), cfg.GetBoundsSize());
    }

    [Test]
    public void CanMerge_WithMergeResult_IsTrue()
    {
      var result = InventoryTestHelpers.Single(level: 2);
      // MergeResult is set via SetTestData → backing field
      var cfg = InventoryTestHelpers.Single(level: 1, mergeResult: result);
      Assert.IsTrue(cfg.CanMerge);
    }

    [Test]
    public void CanMerge_WithoutMergeResult_IsFalse()
    {
      var cfg = InventoryTestHelpers.Single(level: 1, mergeResult: null);
      Assert.IsFalse(cfg.CanMerge);
    }
  }
}
