// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

using Code.Data.StaticData.Configs;

using Code.Infrastructure.AssetManagement;
using Code.Model.Core;
using Code.Model.Services.Inventory.Interfaces;
using Code.Presenter.Bag;
using Code.Presenter.DragDrop;
using Code.UI.Types;
using Code.ViewModel.Cell;

using NSubstitute;

using NUnit.Framework;

using R3;

using UnityEngine;

namespace Code.Tests.EditMode.ViewModel
{
  #region CellViewModel

  [TestFixture]
  public class CellViewModelTests
  {
    private IBagPresenter      _bagPresenter;
    private IDragDropPresenter _dragDropPresenter;
    private IAssetLoader       _assetLoader;
    private CellViewModel      _vm;
    private Vector2Int         _coord = new(2, 3);

    private static readonly Color EmptyColor = new(0.15f, 0.15f, 0.15f, 0.6f);

    [SetUp]
    public void SetUp()
    {
      _bagPresenter      = Substitute.For<IBagPresenter>();
      _dragDropPresenter = Substitute.For<IDragDropPresenter>();
      _assetLoader       = Substitute.For<IAssetLoader>();

      _bagPresenter.OnItemPlaced.Returns(Observable.Empty<InventoryItem>());
      _bagPresenter.OnItemRemoved.Returns(Observable.Empty<InventoryItem>());
      _bagPresenter.OnItemsMerged.Returns(Observable.Empty<MergeResult>());
      _bagPresenter.GetItemAt(Arg.Any<Vector2Int>()).Returns((InventoryItem)null);

      _vm = new CellViewModel(
        _coord, _bagPresenter, _dragDropPresenter, _assetLoader, EmptyColor);
    }

    [TearDown]
    public void TearDown() => _vm.Dispose();

    // ── Initial state ──────────────────────────────────────────────────────

    [Test]
    public void InitialState_EmptyCell_BackgroundIsEmptyColor()
    {
      // Background never changes — always emptyColor regardless of item state
      Assert.AreEqual(EmptyColor, _vm.BackgroundColor.CurrentValue);
    }

    [Test]
    public void InitialState_EmptyCell_OverlayIsTransparent()
    {
      // Overlay is clear (alpha=0) when no item is on the cell
      Assert.AreEqual(Color.clear, _vm.ItemOverlayColor.CurrentValue);
    }

    [Test]
    public void InitialState_HighlightIsNone()
    {
      Assert.AreEqual(HighlightState.None, _vm.Highlight.CurrentValue);
    }

    // ── ItemOverlayColor ───────────────────────────────────────────────────

    [Test]
    public void ItemOverlayColor_WhenCellOccupied_ShowsItemColor()
    {
      var placedSubject = new Subject<InventoryItem>();
      _bagPresenter.OnItemPlaced.Returns(placedSubject);
      _bagPresenter.OnItemRemoved.Returns(Observable.Empty<InventoryItem>());
      _bagPresenter.OnItemsMerged.Returns(Observable.Empty<MergeResult>());
      _bagPresenter.GetItemAt(Arg.Any<Vector2Int>()).Returns((InventoryItem)null);

      var vm = new CellViewModel(
        _coord, _bagPresenter, _dragDropPresenter, _assetLoader, EmptyColor);

      var cfg  = MakeCfg(Color.red);
      var item = new InventoryItem(cfg, _coord);
      _bagPresenter.GetItemAt(_coord).Returns(item);

      placedSubject.OnNext(item);

      Assert.AreEqual(Color.red, vm.ItemOverlayColor.CurrentValue);
      vm.Dispose();
    }

    [Test]
    public void ItemOverlayColor_WhenCellBecomesEmpty_IsTransparent()
    {
      var removedSubject = new Subject<InventoryItem>();
      _bagPresenter.OnItemPlaced.Returns(Observable.Empty<InventoryItem>());
      _bagPresenter.OnItemRemoved.Returns(removedSubject);
      _bagPresenter.OnItemsMerged.Returns(Observable.Empty<MergeResult>());

      var cfg  = MakeCfg(Color.green);
      var item = new InventoryItem(cfg, _coord);
      _bagPresenter.GetItemAt(_coord).Returns(item);

      var vm = new CellViewModel(
        _coord, _bagPresenter, _dragDropPresenter, _assetLoader, EmptyColor);

      // Now remove the item
      _bagPresenter.GetItemAt(_coord).Returns((InventoryItem)null);
      removedSubject.OnNext(item);

      Assert.AreEqual(Color.clear, vm.ItemOverlayColor.CurrentValue);
      vm.Dispose();
    }

    [Test]
    public void BackgroundColor_NeverChanges_WhenItemPlaced()
    {
      var placedSubject = new Subject<InventoryItem>();
      _bagPresenter.OnItemPlaced.Returns(placedSubject);
      _bagPresenter.OnItemRemoved.Returns(Observable.Empty<InventoryItem>());
      _bagPresenter.OnItemsMerged.Returns(Observable.Empty<MergeResult>());
      _bagPresenter.GetItemAt(Arg.Any<Vector2Int>()).Returns((InventoryItem)null);

      var vm = new CellViewModel(
        _coord, _bagPresenter, _dragDropPresenter, _assetLoader, EmptyColor);

      int bgChanges = 0;
      vm.BackgroundColor.Skip(1).Subscribe(_ => bgChanges++);

      var item = new InventoryItem(MakeCfg(Color.blue), _coord);
      _bagPresenter.GetItemAt(_coord).Returns(item);
      placedSubject.OnNext(item);

      // Background must stay the same — item color goes to overlay, not background
      Assert.AreEqual(0, bgChanges);
      Assert.AreEqual(EmptyColor, vm.BackgroundColor.CurrentValue);
      vm.Dispose();
    }

    [Test]
    public void OnItemPlaced_TriggersOverlayColorRefresh()
    {
      var placedSubject = new Subject<InventoryItem>();
      _bagPresenter.OnItemPlaced.Returns(placedSubject);
      _bagPresenter.OnItemRemoved.Returns(Observable.Empty<InventoryItem>());
      _bagPresenter.OnItemsMerged.Returns(Observable.Empty<MergeResult>());
      _bagPresenter.GetItemAt(Arg.Any<Vector2Int>()).Returns((InventoryItem)null);

      var vm = new CellViewModel(
        _coord, _bagPresenter, _dragDropPresenter, _assetLoader, EmptyColor);

      int changes = 0;
      vm.ItemOverlayColor.Skip(1).Subscribe(_ => changes++);

      var cfg  = MakeCfg(Color.red);
      var item = new InventoryItem(cfg, _coord);
      _bagPresenter.GetItemAt(_coord).Returns(item);
      placedSubject.OnNext(item);

      Assert.Greater(changes, 0);
      vm.Dispose();
    }

    // ── Highlight ──────────────────────────────────────────────────────────

    [Test]
    public void SetHighlight_Valid_UpdatesReactiveProperty()
    {
      _vm.SetHighlight(HighlightState.Valid);
      Assert.AreEqual(HighlightState.Valid, _vm.Highlight.CurrentValue);
    }

    [Test]
    public void SetHighlight_None_ResetsToNone()
    {
      _vm.SetHighlight(HighlightState.Merge);
      _vm.SetHighlight(HighlightState.None);
      Assert.AreEqual(HighlightState.None, _vm.Highlight.CurrentValue);
    }

    [Test]
    public void SetHighlight_FiresReactivePropertyChange()
    {
      HighlightState? received = null;
      _vm.Highlight.Skip(1).Subscribe(s => received = s);

      _vm.SetHighlight(HighlightState.Invalid);

      Assert.AreEqual(HighlightState.Invalid, received);
    }

    // ── Commands ───────────────────────────────────────────────────────────

    [Test]
    public void OnBeginDrag_CallsPresenterStartDragFromBag()
    {
      _vm.OnBeginDrag(new Vector2(10f, 20f));
      _dragDropPresenter.Received(1).StartDragFromBag(_coord, new Vector2(10f, 20f));
    }

    [Test]
    public void OnDrag_CallsPresenterUpdateDragPosition()
    {
      _vm.OnDrag(new Vector2(30f, 40f));
      _dragDropPresenter.Received(1).UpdateDragPosition(new Vector2(30f, 40f));
    }

    [Test]
    public void OnEndDrag_CallsPresenterHandleEndDrag()
    {
      _vm.OnEndDrag();
      _dragDropPresenter.Received(1).HandleEndDrag();
    }

    [Test]
    public void OnDrop_CallsPresenterHandleDropOnCell()
    {
      _vm.OnDrop();
      _dragDropPresenter.Received(1).HandleDropOnCell(_coord);
    }

    [Test]
    public void OnPointerEnter_CallsPresenterHandlePointerEnterCell()
    {
      _vm.OnPointerEnter();
      _dragDropPresenter.Received(1).HandlePointerEnterCell(_coord);
    }

    [Test]
    public void OnPointerExit_CallsPresenterHandlePointerExitCell()
    {
      _vm.OnPointerExit();
      _dragDropPresenter.Received(1).HandlePointerExitCell(_coord);
    }

    // ── Dispose ────────────────────────────────────────────────────────────

    [Test]
    public void Dispose_DoesNotThrow() =>
      Assert.DoesNotThrow(() => _vm.Dispose());

    [Test]
    public void Dispose_Twice_DoesNotThrow()
    {
      _vm.Dispose();
      Assert.DoesNotThrow(() => _vm.Dispose());
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static ItemConfig MakeCfg(Color color = default)
    {
      var cfg = ScriptableObject.CreateInstance<ItemConfig>();
      var type = typeof(ItemConfig);
      void Set(string prop, object val) =>
        type.GetField($"<{prop}>k__BackingField",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
          ?.SetValue(cfg, val);

      Set("ItemId",    "test");
      Set("ItemColor", color == default ? Color.white : color);
      Set("Shape",     new List<Vector2Int> { Vector2Int.zero });
      return cfg;
    }
  }

  #endregion
}
