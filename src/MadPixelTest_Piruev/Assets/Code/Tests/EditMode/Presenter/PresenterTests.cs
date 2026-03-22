// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

using Code.Data.StaticData;
using Code.Model.Core;
using Code.Model.Services.BottomSlots.Interfaces;
using Code.Model.Services.DragDrop.Interfaces;
using Code.Model.Services.Inventory.Interfaces;
using Code.Presenter.Bag;
using Code.Presenter.BottomSlots;
using Code.Presenter.DragDrop;
using Code.UI.Types;
using Code.ViewModel.DragIcon;

using NSubstitute;

using NUnit.Framework;

using R3;

using UnityEngine;

namespace Code.Tests.EditMode.Presenter
{
  #region BottomSlotsPresenter

  [TestFixture]
  public class BottomSlotsPresenterTests
  {
    private IBottomSlotsService _slotsService;
    private BottomSlotsPresenter _presenter;

    [SetUp]
    public void SetUp()
    {
      _slotsService = Substitute.For<IBottomSlotsService>();
      _slotsService.OnSlotChanged.Returns(Observable.Empty<int>());
      _slotsService.SlotCount.Returns(5);

      _presenter = new BottomSlotsPresenter(_slotsService);
    }

    [Test]
    public void SlotCount_DelegatesToService() =>
      Assert.AreEqual(5, _presenter.SlotCount);

    [Test]
    public void OnSlotChanged_DelegatesToService()
    {
      var subject = new Subject<int>();
      _slotsService.OnSlotChanged.Returns(subject);
      var presenter = new BottomSlotsPresenter(_slotsService);

      int received = -1;
      presenter.OnSlotChanged.Subscribe(i => received = i);
      subject.OnNext(3);

      Assert.AreEqual(3, received);
    }

    [Test]
    public void GetSlot_DelegatesToService()
    {
      var item = MakeItem();
      _slotsService.GetSlot(2).Returns(item);
      Assert.AreSame(item, _presenter.GetSlot(2));
    }

    [Test]
    public void IsSlotEmpty_DelegatesToService()
    {
      _slotsService.IsSlotEmpty(0).Returns(true);
      Assert.IsTrue(_presenter.IsSlotEmpty(0));
    }

    [Test]
    public void FindFirstFreeSlot_DelegatesToService()
    {
      _slotsService.FindFirstFreeSlot().Returns(3);
      Assert.AreEqual(3, _presenter.FindFirstFreeSlot());
    }

    [Test]
    public void TryPlace_DelegatesToService()
    {
      var item = MakeItem();
      _slotsService.TryPlace(item, 1).Returns(true);
      Assert.IsTrue(_presenter.TryPlace(item, 1));
    }

    [Test]
    public void TryRemove_DelegatesToService()
    {
      var item = MakeItem();
      _slotsService.TryRemove(2, out Arg.Any<InventoryItem>())
        .Returns(ci => { ci[1] = item; return true; });

      bool result = _presenter.TryRemove(2, out var removed);

      Assert.IsTrue(result);
      Assert.AreSame(item, removed);
    }

    [Test]
    public void TryPlaceInFirstFreeSlot_DelegatesToService()
    {
      var item = MakeItem();
      _slotsService.TryPlaceInFirstFreeSlot(item, out Arg.Any<int>())
        .Returns(ci => { ci[1] = 4; return true; });

      bool result = _presenter.TryPlaceInFirstFreeSlot(item, out int idx);

      Assert.IsTrue(result);
      Assert.AreEqual(4, idx);
    }

    private static InventoryItem MakeItem()
    {
      var cfg = ScriptableObject.CreateInstance<ItemConfig>();
      return new InventoryItem(cfg, Vector2Int.zero);
    }
  }
  #endregion

  #region DragDropPresenter

  [TestFixture]
  public class DragDropPresenterTests
  {
    private IBagPresenter _bagPresenter;
    private IBottomSlotsPresenter _slotsPresenter;
    private IGridDragDropService _dragDropService;
    private IDragIconViewModel _dragIconViewModel;
    private DragDropPresenter _presenter;

    [SetUp]
    public void SetUp()
    {
      _bagPresenter = Substitute.For<IBagPresenter>();
      _slotsPresenter = Substitute.For<IBottomSlotsPresenter>();
      _dragDropService = Substitute.For<IGridDragDropService>();
      _dragIconViewModel = Substitute.For<IDragIconViewModel>();

      // Observable stubs
      _bagPresenter.OnItemPlaced.Returns(Observable.Empty<InventoryItem>());
      _bagPresenter.OnItemRemoved.Returns(Observable.Empty<InventoryItem>());
      _bagPresenter.OnItemsMerged.Returns(Observable.Empty<MergeResult>());

      _presenter = new DragDropPresenter(
        _bagPresenter,
        _slotsPresenter,
        _dragDropService,
        _dragIconViewModel,
        Substitute.For<Code.Infrastructure.AssetManagement.IAssetLoader>());
    }

    #region StartDragFromBag

    [Test]
    public void StartDragFromBag_NullItem_DoesNotStartDrag()
    {
      _bagPresenter.GetItemAt(Arg.Any<Vector2Int>()).Returns((InventoryItem)null);

      _presenter.StartDragFromBag(Vector2Int.zero, Vector2.zero);

      _dragDropService.DidNotReceive().StartDrag(
        Arg.Any<InventoryItem>(), Arg.Any<DragSource>(),
        Arg.Any<Vector2Int>(), Arg.Any<int>());
    }

    [Test]
    public void StartDragFromBag_ValidItem_RemovesFromBagAndStartsDrag()
    {
      var origin = new Vector2Int(2, 3);
      var item = MakeItem(origin);
      _bagPresenter.GetItemAt(origin).Returns(item);
      _bagPresenter.TryRemove(item).Returns(true);

      _presenter.StartDragFromBag(origin, Vector2.zero);

      _bagPresenter.Received(1).TryRemove(item);
      _dragDropService.Received(1).StartDrag(
        item, DragSource.Bag, Vector2Int.zero, -1);
    }

    [Test]
    public void StartDragFromBag_DragOffset_CalculatedFromCellMinusOrigin()
    {
      var origin = new Vector2Int(1, 1);
      var grabCell = new Vector2Int(2, 2);
      var item = MakeItem(origin);
      _bagPresenter.GetItemAt(grabCell).Returns(item);
      _bagPresenter.TryRemove(item).Returns(true);

      _presenter.StartDragFromBag(grabCell, Vector2.zero);

      var expectedOffset = grabCell - origin; // (1,1)
      _dragDropService.Received(1).StartDrag(
        item, DragSource.Bag, expectedOffset, -1);
    }

    #endregion

    #region StartDragFromSlot

    [Test]
    public void StartDragFromSlot_FailedRemove_DoesNotStartDrag()
    {
      _slotsPresenter.TryRemove(0, out Arg.Any<InventoryItem>()).Returns(false);

      _presenter.StartDragFromSlot(0, Vector2.zero);

      _dragDropService.DidNotReceive().StartDrag(
        Arg.Any<InventoryItem>(), Arg.Any<DragSource>(),
        Arg.Any<Vector2Int>(), Arg.Any<int>());
    }

    [Test]
    public void StartDragFromSlot_ValidSlot_StartsCorrectDragState()
    {
      var item = MakeItem();
      _slotsPresenter.TryRemove(2, out Arg.Any<InventoryItem>())
        .Returns(ci => { ci[1] = item; return true; });

      _presenter.StartDragFromSlot(2, Vector2.zero);

      _dragDropService.Received(1).StartDrag(
        item, DragSource.BottomSlot, Vector2Int.zero, 2);
    }

    #endregion

    #region UpdateDragPosition

    [Test]
    public void UpdateDragPosition_DelegatesToDragIconViewModel()
    {
      var pos = new Vector2(100f, 200f);
      _presenter.UpdateDragPosition(pos);
      _dragIconViewModel.Received(1).UpdatePosition(pos);
    }

    #endregion

    #region HandleEndDrag

    [Test]
    public void HandleEndDrag_HidesDragIcon()
    {
      _dragDropService.IsDragging.Returns(false);
      _presenter.HandleEndDrag();
      _dragIconViewModel.Received(1).Hide();
    }

    [Test]
    public void HandleEndDrag_BagSource_FreeSlotsAvailable_EndsAndPlacesInSlot()
    {
      var item = MakeItem();
      _dragDropService.IsDragging.Returns(true);
      _dragDropService.Source.Returns(DragSource.Bag);
      _dragDropService.DraggedItem.Returns(item);
      _slotsPresenter.TryPlaceInFirstFreeSlot(item, out Arg.Any<int>()).Returns(true);

      _presenter.HandleEndDrag();

      _slotsPresenter.Received(1).TryPlaceInFirstFreeSlot(item, out Arg.Any<int>());
      _dragDropService.Received(1).EndDrag();
    }

    [Test]
    public void HandleEndDrag_BagSource_NoFreeSlots_CancelsDrag()
    {
      var item = MakeItem();
      _dragDropService.IsDragging.Returns(true);
      _dragDropService.Source.Returns(DragSource.Bag);
      _dragDropService.DraggedItem.Returns(item);
      _slotsPresenter.TryPlaceInFirstFreeSlot(item, out Arg.Any<int>()).Returns(false);

      _presenter.HandleEndDrag();

      _dragDropService.Received(1).CancelDrag();
    }

    [Test]
    public void HandleEndDrag_SlotSource_AlwaysCancels()
    {
      _dragDropService.IsDragging.Returns(true);
      _dragDropService.Source.Returns(DragSource.BottomSlot);

      _presenter.HandleEndDrag();

      _dragDropService.Received(1).CancelDrag();
    }

    #endregion

    #region HandleDropOnCell

    [Test]
    public void HandleDropOnCell_NotDragging_DoesNothing()
    {
      _dragDropService.IsDragging.Returns(false);
      _presenter.HandleDropOnCell(Vector2Int.zero);
      _bagPresenter.DidNotReceive().Merge(Arg.Any<InventoryItem>(), Arg.Any<InventoryItem>());
    }

    [Test]
    public void HandleDropOnCell_MergePossible_MergesAndEndsDrag()
    {
      var dragged = MakeItem(); var target = MakeItem(); var merged = MakeItem();
      SetupDragging(dragged, DragSource.Bag, Vector2Int.zero);

      _bagPresenter
        .CanMerge(dragged, new Vector2Int(1, 1), out Arg.Any<InventoryItem>())
        .Returns(ci => { ci[2] = target; return true; });
      _bagPresenter.Merge(dragged, target).Returns(merged);

      _presenter.HandleDropOnCell(new Vector2Int(1, 1));

      _bagPresenter.Received(1).Merge(dragged, target);
      _dragDropService.Received(1).EndDrag();
    }

    [Test]
    public void HandleDropOnCell_PlacePossible_PlacesAndEndsDrag()
    {
      var dragged = MakeItem(new Vector2Int(0, 0));
      SetupDragging(dragged, DragSource.Bag, Vector2Int.zero);

      _bagPresenter
        .CanMerge(dragged, Arg.Any<Vector2Int>(), out Arg.Any<InventoryItem>())
        .Returns(false);
      _bagPresenter.TryPlace(dragged).Returns(true);

      _presenter.HandleDropOnCell(new Vector2Int(2, 2));

      _bagPresenter.Received(1).TryPlace(dragged);
      _dragDropService.Received(1).EndDrag();
    }

    [Test]
    public void HandleDropOnCell_BothFail_CancelsDrag()
    {
      var dragged = MakeItem(new Vector2Int(0, 0));
      SetupDragging(dragged, DragSource.Bag, Vector2Int.zero);

      _bagPresenter
        .CanMerge(Arg.Any<InventoryItem>(), Arg.Any<Vector2Int>(), out Arg.Any<InventoryItem>())
        .Returns(false);
      _bagPresenter.TryPlace(Arg.Any<InventoryItem>()).Returns(false);

      _presenter.HandleDropOnCell(new Vector2Int(2, 2));

      _dragDropService.Received(1).CancelDrag();
    }

    #endregion

    #region HandleDropOnSlot

    [Test]
    public void HandleDropOnSlot_EmptySlot_PlacesAndEndsDrag()
    {
      var dragged = MakeItem();
      SetupDragging(dragged, DragSource.Bag, Vector2Int.zero);
      _slotsPresenter.GetSlot(1).Returns((InventoryItem)null);
      _slotsPresenter.TryPlace(dragged, 1).Returns(true);

      _presenter.HandleDropOnSlot(1);

      _slotsPresenter.Received(1).TryPlace(dragged, 1);
      _dragDropService.Received(1).EndDrag();
    }

    [Test]
    public void HandleDropOnSlot_OccupiedSlot_SwapSucceeds_EndsDrag()
    {
      var dragged = MakeItem(); var existing = MakeItem();
      SetupDragging(dragged, DragSource.Bag, Vector2Int.zero);
      _slotsPresenter.GetSlot(0).Returns(existing);
      _slotsPresenter.TryRemove(0, out Arg.Any<InventoryItem>())
        .Returns(ci => { ci[1] = existing; return true; });
      _slotsPresenter.TryPlaceInFirstFreeSlot(existing, out Arg.Any<int>()).Returns(true);
      _slotsPresenter.TryPlace(dragged, 0).Returns(true);

      _presenter.HandleDropOnSlot(0);

      _slotsPresenter.Received(1).TryPlaceInFirstFreeSlot(existing, out Arg.Any<int>());
      _slotsPresenter.Received(1).TryPlace(dragged, 0);
      _dragDropService.Received(1).EndDrag();
    }

    [Test]
    public void HandleDropOnSlot_OccupiedSlot_NoFreeSlots_CancelsDrag()
    {
      var dragged = MakeItem(); var existing = MakeItem();
      SetupDragging(dragged, DragSource.Bag, Vector2Int.zero);
      _slotsPresenter.GetSlot(0).Returns(existing);
      _slotsPresenter.TryRemove(0, out Arg.Any<InventoryItem>())
        .Returns(ci => { ci[1] = existing; return true; });
      _slotsPresenter.TryPlaceInFirstFreeSlot(existing, out Arg.Any<int>()).Returns(false);

      _presenter.HandleDropOnSlot(0);

      _dragDropService.Received(1).CancelDrag();
    }

    #endregion


    #region HandlePointerEnterCell

    [Test]
    public void HandlePointerEnterCell_NotDragging_DoesNotRequestHighlight()
    {
      _dragDropService.IsDragging.Returns(false);
      _presenter.HandlePointerEnterCell(Vector2Int.zero);
      _bagPresenter.DidNotReceive().RequestHighlight(
        Arg.Any<ItemConfig>(), Arg.Any<Vector2Int>(), Arg.Any<HighlightState>());
    }

    [Test]
    public void HandlePointerEnterCell_CanMerge_RequestsMergeHighlight()
    {
      var dragged = MakeItem(new Vector2Int(0, 0));
      SetupDragging(dragged, DragSource.Bag, Vector2Int.zero);
      _bagPresenter
        .CanMerge(dragged, new Vector2Int(1, 1), out Arg.Any<InventoryItem>())
        .Returns(true);

      _presenter.HandlePointerEnterCell(new Vector2Int(1, 1));

      _bagPresenter.Received(1).RequestHighlight(
        dragged.Config, new Vector2Int(1, 1), HighlightState.Merge);
    }

    [Test]
    public void HandlePointerEnterCell_CanPlace_RequestsValidHighlight()
    {
      var dragged = MakeItem(new Vector2Int(0, 0));
      SetupDragging(dragged, DragSource.Bag, Vector2Int.zero);
      _bagPresenter
        .CanMerge(dragged, new Vector2Int(2, 2), out Arg.Any<InventoryItem>())
        .Returns(false);
      _bagPresenter
        .CanPlace(dragged.Config, new Vector2Int(2, 2), dragged)
        .Returns(true);

      _presenter.HandlePointerEnterCell(new Vector2Int(2, 2));

      _bagPresenter.Received(1).RequestHighlight(
        dragged.Config, new Vector2Int(2, 2), HighlightState.Valid);
    }

    [Test]
    public void HandlePointerEnterCell_CannotPlaceOrMerge_RequestsInvalidHighlight()
    {
      var dragged = MakeItem(new Vector2Int(0, 0));
      SetupDragging(dragged, DragSource.Bag, Vector2Int.zero);
      _bagPresenter
        .CanMerge(Arg.Any<InventoryItem>(), Arg.Any<Vector2Int>(), out Arg.Any<InventoryItem>())
        .Returns(false);
      _bagPresenter
        .CanPlace(Arg.Any<ItemConfig>(), Arg.Any<Vector2Int>(), Arg.Any<InventoryItem>())
        .Returns(false);

      _presenter.HandlePointerEnterCell(new Vector2Int(3, 3));

      _bagPresenter.Received(1).RequestHighlight(
        Arg.Any<ItemConfig>(), Arg.Any<Vector2Int>(), HighlightState.Invalid);
    }

    #endregion

    #region HandlePointerExitCel

    [Test]
    public void HandlePointerExitCell_ClearsHighlight()
    {
      var dragged = MakeItem(new Vector2Int(0, 0));
      SetupDragging(dragged, DragSource.Bag, Vector2Int.zero);

      _presenter.HandlePointerExitCell(new Vector2Int(1, 1));

      _bagPresenter.Received(1).RequestHighlight(
        Arg.Any<ItemConfig>(), Arg.Any<Vector2Int>(), HighlightState.None);
    }

    #endregion

    #region Helpers

    private void SetupDragging(InventoryItem item, DragSource source, Vector2Int dragOffset)
    {
      _dragDropService.IsDragging.Returns(true);
      _dragDropService.DraggedItem.Returns(item);
      _dragDropService.Source.Returns(source);
      _dragDropService.DragOffset.Returns(dragOffset);
    }

    private static ItemConfig MakeCfg()
    {
      var cfg = ScriptableObject.CreateInstance<ItemConfig>();
      typeof(ItemConfig)
        .GetField("<Shape>k__BackingField",
          System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
        ?.SetValue(cfg, new List<Vector2Int> { Vector2Int.zero });
      return cfg;
    }

    private static InventoryItem MakeItem(Vector2Int origin = default) =>
      new InventoryItem(MakeCfg(), origin);
  }
    #endregion

  #endregion
}
