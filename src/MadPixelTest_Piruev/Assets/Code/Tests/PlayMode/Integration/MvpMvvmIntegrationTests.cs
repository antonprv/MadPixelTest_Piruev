// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

using Code.Data.StaticData.Configs;

using Code.Infrastructure.AssetManagement;
using Code.Infrastructure.Services;
using Code.Infrastructure.Services.StaticData.Interfaces;
using Code.Model.Core;
using Code.Model.Services.BottomSlots;
using Code.Model.Services.DragDrop;
using Code.Model.Services.Inventory;
using Code.Presenter.Bag;
using Code.Presenter.BottomSlots;
using Code.Presenter.DragDrop;
using Code.UI.Types;
using Code.ViewModel.Bag;
using Code.ViewModel.Cell;
using Code.ViewModel.DragIcon;

using NSubstitute;

using NUnit.Framework;

using R3;

using UnityEngine;

namespace Code.Tests.PlayMode.Integration
{
  /// <summary>
  /// Integration tests for the full MVP+MVVM chain.
  /// All services + presenters + viewmodels are real instances.
  /// IAssetLoader is mocked (requires Addressables runtime).
  /// IBagConfigSubservice is mocked — keeps tests independent of ScriptableObject assets.
  ///
  /// Key invariant after the visual refactor:
  ///   CellViewModel.BackgroundColor is ALWAYS emptyColor — it never changes.
  ///   Item presence is indicated by ItemOverlayColor (clear when empty, ItemColor when occupied).
  /// </summary>
  [TestFixture]
  public class MvpMvvmIntegrationTests
  {
    #region Real instances

    private GridInventoryService _inventoryService;
    private BottomSlotsService   _slotsService;
    private GridDragDropService  _dragDropService;
    private BagPresenter         _bagPresenter;
    private BottomSlotsPresenter _slotsPresenter;
    private DragIconViewModel    _dragIconVm;
    private DragDropPresenter    _dragDropPresenter;
    private BagViewModel         _bagViewModel;

    private static readonly Color EmptyColor = new(0.15f, 0.15f, 0.15f, 0.6f);

    #endregion

    #region Mocked

    private IAssetLoader _assetLoader;

    #endregion

    #region Data

    private ItemConfig _singleLv1;
    private ItemConfig _singleLv2;
    private ItemConfig _lShapeCfg;

    #endregion

    [SetUp]
    public void SetUp()
    {
      _assetLoader = Substitute.For<IAssetLoader>();

      // Clear locator between tests
      SlotPositionProviderLocator.Register(null);

      var bagConfig = MakeBagConfig(5, 7, 5);

      _inventoryService = new GridInventoryService(bagConfig);
      _slotsService     = new BottomSlotsService(bagConfig);
      _inventoryService.Initialize();
      _slotsService.Initialize();

      _dragDropService = new GridDragDropService(_inventoryService, _slotsService);

      _bagPresenter   = new BagPresenter(_inventoryService);
      _slotsPresenter = new BottomSlotsPresenter(_slotsService);
      _dragIconVm     = new DragIconViewModel();

      _dragDropPresenter = new DragDropPresenter(
        _bagPresenter, _slotsPresenter, _dragDropService, _dragIconVm, _assetLoader);

      _bagViewModel = new BagViewModel(
        bagConfig, _bagPresenter, _dragDropPresenter, _assetLoader);

      _singleLv2 = MakeCfg("s_lv2", 2, new List<Vector2Int> { Vector2Int.zero });
      _singleLv1 = MakeCfg("s_lv1", 1,
        new List<Vector2Int> { Vector2Int.zero }, merge: _singleLv2);
      _lShapeCfg = MakeCfg("lshape", 1,
        new List<Vector2Int> { new(0, 0), new(0, 1), new(1, 1) });
    }

    [TearDown]
    public void TearDown()
    {
      _bagViewModel.Dispose();
      SlotPositionProviderLocator.Register(null);
    }

    // ── Drag from slot → drop in bag ───────────────────────────────────────

    [Test]
    public void DragFromSlot_DropInBag_CellOverlayReflectsNewItem()
    {
      // Background is always emptyColor — check overlay instead
      var item   = new InventoryItem(_singleLv1, Vector2Int.zero);
      var target = new Vector2Int(2, 3);
      _slotsService.TryPlace(item, 0);

      _dragDropPresenter.StartDragFromSlot(0, Vector2.zero);
      _dragDropPresenter.HandleDropOnCell(target);

      var cellVm = (CellViewModel)_bagViewModel.GetCellViewModel(target);
      Assert.AreEqual(_singleLv1.ItemColor, cellVm.ItemOverlayColor.CurrentValue);
    }

    [Test]
    public void DragFromSlot_DropInBag_BackgroundRemainsEmptyColor()
    {
      // Background must NEVER change regardless of item placement
      var item   = new InventoryItem(_singleLv1, Vector2Int.zero);
      var target = new Vector2Int(2, 3);
      _slotsService.TryPlace(item, 0);

      _dragDropPresenter.StartDragFromSlot(0, Vector2.zero);
      _dragDropPresenter.HandleDropOnCell(target);

      var cellVm = (CellViewModel)_bagViewModel.GetCellViewModel(target);
      Assert.AreEqual(EmptyColor, cellVm.BackgroundColor.CurrentValue);
    }

    // ── Merge triggers animation ───────────────────────────────────────────

    [Test]
    public void Merge_TriggersMergeAnimation_OnBagViewModel()
    {
      var a = new InventoryItem(_singleLv1, new Vector2Int(0, 0));
      var b = new InventoryItem(_singleLv1, new Vector2Int(1, 0));
      _inventoryService.TryPlace(a);
      _inventoryService.TryPlace(b);

      Vector2Int? animOrigin = null;
      _bagViewModel.OnMergeAnimation.Subscribe(o => animOrigin = o);

      _inventoryService.TryRemove(b);
      _dragDropService.StartDrag(b, DragSource.Bag, Vector2Int.zero);
      _dragDropPresenter.HandleDropOnCell(new Vector2Int(0, 0));

      Assert.IsNotNull(animOrigin);
      Assert.AreEqual(new Vector2Int(0, 0), animOrigin.Value);
    }

    // ── Highlight routing ──────────────────────────────────────────────────

    [Test]
    public void PointerEnterCell_CanPlace_SetsValidHighlightOnCellViewModel()
    {
      var bagConfig = MakeBagConfig(5, 7, 5);
      var ddService = new GridDragDropService(_inventoryService, _slotsService);
      var ddp       = new DragDropPresenter(
        _bagPresenter, _slotsPresenter, ddService, _dragIconVm, _assetLoader);

      var bagVm = new BagViewModel(bagConfig, _bagPresenter, ddp, _assetLoader);

      var dragItem = new InventoryItem(_singleLv1, new Vector2Int(1, 0));
      _inventoryService.TryPlace(dragItem);
      ddp.StartDragFromBag(new Vector2Int(1, 0), Vector2.zero);
      ddp.HandlePointerEnterCell(new Vector2Int(0, 0));

      var cellVm = bagVm.GetCellViewModel(new Vector2Int(0, 0));
      Assert.AreEqual(HighlightState.Valid, cellVm.Highlight.CurrentValue);

      bagVm.Dispose();
    }

    // ── DragIconViewModel state ────────────────────────────────────────────

    [Test]
    public void HandleEndDrag_WhenNotDragging_HidesDragIconViewModel()
    {
      // No drag active — HandleEndDrag should call Hide immediately
      _dragDropPresenter.HandleEndDrag();
      Assert.IsFalse(_dragIconVm.IsVisible.CurrentValue);
    }

    // ── ItemOverlayColor on cancel ─────────────────────────────────────────

    [Test]
    public void CancelDrag_FromBag_ItemReturnsToBag_OverlayReflectsItem()
    {
      var origin = new Vector2Int(0, 0);
      var item   = new InventoryItem(_singleLv1, origin);
      _inventoryService.TryPlace(item);

      _dragDropPresenter.StartDragFromBag(origin, Vector2.zero);
      _dragDropService.CancelDrag();

      var cellVm = (CellViewModel)_bagViewModel.GetCellViewModel(origin);
      // Item returned to bag → overlay shows item color
      Assert.AreEqual(_singleLv1.ItemColor, cellVm.ItemOverlayColor.CurrentValue);
    }

    [Test]
    public void CancelDrag_FromBag_BackgroundAlwaysEmptyColor()
    {
      var origin = new Vector2Int(0, 0);
      var item   = new InventoryItem(_singleLv1, origin);
      _inventoryService.TryPlace(item);

      _dragDropPresenter.StartDragFromBag(origin, Vector2.zero);
      _dragDropService.CancelDrag();

      var cellVm = (CellViewModel)_bagViewModel.GetCellViewModel(origin);
      Assert.AreEqual(EmptyColor, cellVm.BackgroundColor.CurrentValue);
    }

    // ── OnItemPlaced / OnItemRemoved on BagViewModel ───────────────────────

    [Test]
    public void BagViewModel_OnItemPlaced_FiresWhenItemPlaced()
    {
      InventoryItem received = null;
      _bagViewModel.OnItemPlaced.Subscribe(i => received = i);

      var item = new InventoryItem(_singleLv1, Vector2Int.zero);
      _inventoryService.TryPlace(item);

      Assert.AreSame(item, received);
    }

    [Test]
    public void BagViewModel_OnItemRemoved_FiresWhenItemRemoved()
    {
      var item = new InventoryItem(_singleLv1, Vector2Int.zero);
      _inventoryService.TryPlace(item);

      InventoryItem received = null;
      _bagViewModel.OnItemRemoved.Subscribe(i => received = i);

      _inventoryService.TryRemove(item);

      Assert.AreSame(item, received);
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static IBagConfigSubservice MakeBagConfig(int w, int h, int slots)
    {
      var mock = Substitute.For<IBagConfigSubservice>();
      mock.GridSize.Returns(new Vector2Int(w, h));
      mock.BottomSlotCount.Returns(slots);
      mock.CellSize.Returns(80f);
      mock.CellSpacing.Returns(4f);

      var cells = new HashSet<Vector2Int>();
      for (int x = 0; x < w; x++)
        for (int y = 0; y < h; y++)
          cells.Add(new Vector2Int(x, y));
      mock.GetActiveCellsSet().Returns(cells);
      return mock;
    }

    private static ItemConfig MakeCfg(
      string id, int level, List<Vector2Int> shape, ItemConfig merge = null)
    {
      var cfg = ScriptableObject.CreateInstance<ItemConfig>();
      void Set(string prop, object val) =>
        typeof(ItemConfig)
          .GetField($"<{prop}>k__BackingField",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
          ?.SetValue(cfg, val);
      Set("ItemId",      id);
      Set("Level",       level);
      Set("Shape",       shape);
      Set("MergeResult", merge);
      Set("ItemColor",   Color.white);
      return cfg;
    }
  }
}
