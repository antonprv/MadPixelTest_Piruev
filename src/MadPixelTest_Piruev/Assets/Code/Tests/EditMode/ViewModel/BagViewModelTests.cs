// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

using Code.Data.StaticData.Configs;

using Code.Infrastructure.AssetManagement;
using Code.Infrastructure.Services.StaticData.Interfaces;
using Code.Model.Core;
using Code.Model.Services.Inventory.Interfaces;
using Code.Presenter.Bag;
using Code.Presenter.DragDrop;
using Code.UI.Types;
using Code.ViewModel.Bag;

using NSubstitute;

using NUnit.Framework;

using R3;

using UnityEngine;

namespace Code.Tests.EditMode.ViewModel
{
  #region BagViewModel

  [TestFixture]
  public class BagViewModelTests
  {
    private IBagPresenter        _bagPresenter;
    private IDragDropPresenter   _dragDropPresenter;
    private IAssetLoader         _assetLoader;
    private IBagConfigSubservice _bagConfig;
    private BagViewModel         _vm;

    private Subject<HighlightRequest> _highlightSubject;
    private Subject<MergeResult>      _mergeSubject;
    private Subject<InventoryItem>    _placedSubject;
    private Subject<InventoryItem>    _removedSubject;

    [SetUp]
    public void SetUp()
    {
      _bagPresenter      = Substitute.For<IBagPresenter>();
      _dragDropPresenter = Substitute.For<IDragDropPresenter>();
      _assetLoader       = Substitute.For<IAssetLoader>();
      _bagConfig         = MakeBagConfig(3, 4);

      _highlightSubject = new Subject<HighlightRequest>();
      _mergeSubject     = new Subject<MergeResult>();
      _placedSubject    = new Subject<InventoryItem>();
      _removedSubject   = new Subject<InventoryItem>();

      _bagPresenter.OnItemPlaced.Returns(_placedSubject);
      _bagPresenter.OnItemRemoved.Returns(_removedSubject);
      _bagPresenter.OnItemsMerged.Returns(_mergeSubject);
      _bagPresenter.OnHighlightRequested.Returns(_highlightSubject);
      _bagPresenter.GetItemAt(Arg.Any<Vector2Int>()).Returns((InventoryItem)null);
      _bagPresenter.GetAllItems().Returns(new List<InventoryItem>());

      _vm = new BagViewModel(_bagConfig, _bagPresenter, _dragDropPresenter, _assetLoader);
    }

    [TearDown]
    public void TearDown() => _vm.Dispose();

    // ── Layout ─────────────────────────────────────────────────────────────

    [Test]
    public void GridSize_MatchesBagConfig() =>
      Assert.AreEqual(new Vector2Int(3, 4), _vm.GridSize);

    [Test]
    public void CellSize_MatchesBagConfig() =>
      Assert.AreEqual(80f, _vm.CellSize);

    [Test]
    public void CellSpacing_MatchesBagConfig() =>
      Assert.AreEqual(4f, _vm.CellSpacing);

    [Test]
    public void ActiveCells_MatchesBagConfigActiveCells()
    {
      Assert.AreEqual(3 * 4, _vm.ActiveCells.Count);
      Assert.IsTrue(_vm.ActiveCells.Contains(new Vector2Int(0, 0)));
      Assert.IsTrue(_vm.ActiveCells.Contains(new Vector2Int(2, 3)));
    }

    // ── CellViewModels ─────────────────────────────────────────────────────

    [Test]
    public void GetCellViewModel_ValidCoord_ReturnsNonNull() =>
      Assert.IsNotNull(_vm.GetCellViewModel(new Vector2Int(0, 0)));

    [Test]
    public void GetCellViewModel_OutOfRange_ReturnsNull() =>
      Assert.IsNull(_vm.GetCellViewModel(new Vector2Int(99, 99)));

    [Test]
    public void GetCellViewModel_AllGridCoords_HaveCellViewModels()
    {
      for (int x = 0; x < 3; x++)
        for (int y = 0; y < 4; y++)
          Assert.IsNotNull(_vm.GetCellViewModel(new Vector2Int(x, y)));
    }

    // ── OnItemPlaced / OnItemRemoved forwarding ────────────────────────────

    [Test]
    public void OnItemPlaced_ForwardsPresenterEvent()
    {
      InventoryItem received = null;
      _vm.OnItemPlaced.Subscribe(i => received = i);

      var item = MakeItem();
      _placedSubject.OnNext(item);

      Assert.AreSame(item, received);
    }

    [Test]
    public void OnItemRemoved_ForwardsPresenterEvent()
    {
      InventoryItem received = null;
      _vm.OnItemRemoved.Subscribe(i => received = i);

      var item = MakeItem();
      _removedSubject.OnNext(item);

      Assert.AreSame(item, received);
    }

    // ── GetAllItems ────────────────────────────────────────────────────────

    [Test]
    public void GetAllItems_DelegatesToPresenter()
    {
      var list = new List<InventoryItem> { MakeItem(), MakeItem() };
      _bagPresenter.GetAllItems().Returns(list);

      var result = _vm.GetAllItems();

      Assert.AreSame(list, result);
    }

    // ── Highlight routing ──────────────────────────────────────────────────

    [Test]
    public void HighlightRequest_RoutesToCorrectCellViewModel()
    {
      var cfg    = MakeCfg(new List<Vector2Int> { Vector2Int.zero });
      var origin = new Vector2Int(1, 1);
      var cellVm = _vm.GetCellViewModel(origin);

      HighlightState? received = null;
      cellVm.Highlight.Skip(1).Subscribe(s => received = s);

      _highlightSubject.OnNext(new HighlightRequest(cfg, origin, HighlightState.Valid));

      Assert.AreEqual(HighlightState.Valid, received);
    }

    [Test]
    public void HighlightRequest_MultiCellShape_RoutesToAllCells()
    {
      var shape  = new List<Vector2Int> { new(0, 0), new(0, 1), new(1, 1) };
      var cfg    = MakeCfg(shape);
      var origin = Vector2Int.zero;
      var states = new Dictionary<Vector2Int, HighlightState>();

      foreach (var offset in shape)
      {
        var coord  = origin + offset;
        var cellVm = _vm.GetCellViewModel(coord);
        cellVm?.Highlight.Skip(1).Subscribe(s => states[coord] = s);
      }

      _highlightSubject.OnNext(new HighlightRequest(cfg, origin, HighlightState.Merge));

      foreach (var offset in shape)
      {
        var coord = origin + offset;
        if (_vm.GetCellViewModel(coord) != null)
          Assert.AreEqual(HighlightState.Merge, states[coord]);
      }
    }

    // ── Merge animation ────────────────────────────────────────────────────

    [Test]
    public void OnMergeAnimation_Fires_WhenItemsMerged()
    {
      Vector2Int? received = null;
      _vm.OnMergeAnimation.Subscribe(o => received = o);

      var mergedItem = new InventoryItem(
        MakeCfg(new List<Vector2Int> { Vector2Int.zero }), new Vector2Int(1, 2));
      var a = new InventoryItem(MakeCfg(new List<Vector2Int> { Vector2Int.zero }), Vector2Int.zero);

      _mergeSubject.OnNext(new MergeResult(a, a, mergedItem));

      Assert.IsNotNull(received);
      Assert.AreEqual(new Vector2Int(1, 2), received.Value);
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static IBagConfigSubservice MakeBagConfig(int w, int h)
    {
      var mock = Substitute.For<IBagConfigSubservice>();
      mock.GridSize.Returns(new Vector2Int(w, h));
      mock.BottomSlotCount.Returns(5);
      mock.CellSize.Returns(80f);
      mock.CellSpacing.Returns(4f);

      var cells = new HashSet<Vector2Int>();
      for (int x = 0; x < w; x++)
        for (int y = 0; y < h; y++)
          cells.Add(new Vector2Int(x, y));
      mock.GetActiveCellsSet().Returns(cells);
      return mock;
    }

    private static ItemConfig MakeCfg(List<Vector2Int> shape)
    {
      var cfg = ScriptableObject.CreateInstance<ItemConfig>();
      typeof(ItemConfig)
        .GetField("<Shape>k__BackingField",
          System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
        ?.SetValue(cfg, shape);
      return cfg;
    }

    private static InventoryItem MakeItem() =>
      new InventoryItem(
        MakeCfg(new List<Vector2Int> { Vector2Int.zero }),
        Vector2Int.zero);
  }

  #endregion
}
