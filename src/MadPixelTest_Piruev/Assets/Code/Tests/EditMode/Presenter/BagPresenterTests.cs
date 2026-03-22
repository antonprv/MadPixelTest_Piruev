// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

using Code.Data.StaticData;
using Code.Model.Core;
using Code.Model.Services.Inventory.Interfaces;
using Code.Presenter.Bag;
using Code.UI.Types;

using NSubstitute;

using NUnit.Framework;

using R3;

using UnityEngine;

namespace Code.Tests.EditMode.Presenter
{
  [TestFixture]
  public class BagPresenterTests
  {
    private IGridInventoryService _inventoryService;
    private BagPresenter _presenter;

    [SetUp]
    public void SetUp()
    {
      _inventoryService = Substitute.For<IGridInventoryService>();

      // Return empty observables so Subscribe doesn't throw
      _inventoryService.OnItemPlaced.Returns(Observable.Empty<InventoryItem>());
      _inventoryService.OnItemRemoved.Returns(Observable.Empty<InventoryItem>());
      _inventoryService.OnItemsMerged.Returns(Observable.Empty<MergeResult>());

      _presenter = new BagPresenter(_inventoryService);
    }

    #region Observable pass-through

    [Test]
    public void OnItemPlaced_DelegatesToService()
    {
      var subject = new Subject<InventoryItem>();
      _inventoryService.OnItemPlaced.Returns(subject);
      var presenter = new BagPresenter(_inventoryService);

      InventoryItem received = null;
      presenter.OnItemPlaced.Subscribe(i => received = i);

      var item = MakeItem();
      subject.OnNext(item);

      Assert.AreSame(item, received);
    }

    [Test]
    public void OnItemRemoved_DelegatesToService()
    {
      var subject = new Subject<InventoryItem>();
      _inventoryService.OnItemRemoved.Returns(subject);
      var presenter = new BagPresenter(_inventoryService);

      InventoryItem received = null;
      presenter.OnItemRemoved.Subscribe(i => received = i);

      var item = MakeItem();
      subject.OnNext(item);

      Assert.AreSame(item, received);
    }

    [Test]
    public void OnItemsMerged_DelegatesToService()
    {
      var subject = new Subject<MergeResult>();
      _inventoryService.OnItemsMerged.Returns(subject);
      var presenter = new BagPresenter(_inventoryService);

      MergeResult? received = null;
      presenter.OnItemsMerged.Subscribe(r => received = r);

      var a = MakeItem(); var b = MakeItem(); var c = MakeItem();
      subject.OnNext(new MergeResult(a, b, c));

      Assert.IsNotNull(received);
    }
    #endregion

    #region Placement delegation

    [Test]
    public void CanPlace_DelegatesToService()
    {
      var cfg = MakeCfg();
      _inventoryService.CanPlace(cfg, Vector2Int.zero, null).Returns(true);

      Assert.IsTrue(_presenter.CanPlace(cfg, Vector2Int.zero));
      _inventoryService.Received(1).CanPlace(cfg, Vector2Int.zero, null);
    }

    [Test]
    public void TryPlace_DelegatesToService()
    {
      var item = MakeItem();
      _inventoryService.TryPlace(item).Returns(true);

      Assert.IsTrue(_presenter.TryPlace(item));
      _inventoryService.Received(1).TryPlace(item);
    }

    [Test]
    public void TryRemove_DelegatesToService()
    {
      var item = MakeItem();
      _inventoryService.TryRemove(item).Returns(true);

      Assert.IsTrue(_presenter.TryRemove(item));
      _inventoryService.Received(1).TryRemove(item);
    }
    #endregion

    #region Query delegation

    [Test]
    public void GetItemAt_DelegatesToService()
    {
      var item = MakeItem();
      _inventoryService.GetItemAt(Vector2Int.zero).Returns(item);

      Assert.AreSame(item, _presenter.GetItemAt(Vector2Int.zero));
    }

    [Test]
    public void GetAllItems_DelegatesToService()
    {
      var list = new List<InventoryItem> { MakeItem() };
      _inventoryService.GetAllItems().Returns(list);

      Assert.AreSame(list, _presenter.GetAllItems());
    }
    #endregion

    #region Merge delegation

    [Test]
    public void CanMerge_DelegatesToService()
    {
      var dragged = MakeItem();
      var target = MakeItem();
      _inventoryService
        .CanMerge(dragged, Vector2Int.zero, out Arg.Any<InventoryItem>())
        .Returns(ci => { ci[2] = target; return true; });

      bool result = _presenter.CanMerge(dragged, Vector2Int.zero, out var found);

      Assert.IsTrue(result);
      Assert.AreSame(target, found);
    }

    [Test]
    public void Merge_DelegatesToService()
    {
      var a = MakeItem(); var b = MakeItem(); var merged = MakeItem();
      _inventoryService.Merge(a, b).Returns(merged);

      Assert.AreSame(merged, _presenter.Merge(a, b));
    }
    #endregion

    #region Highlight

    [Test]
    public void RequestHighlight_FiresOnHighlightRequested()
    {
      HighlightRequest? received = null;
      _presenter.OnHighlightRequested.Subscribe(r => received = r);

      var cfg = MakeCfg();
      _presenter.RequestHighlight(cfg, new Vector2Int(1, 1), HighlightState.Valid);

      Assert.IsNotNull(received);
      Assert.AreSame(cfg, received.Value.Config);
      Assert.AreEqual(new Vector2Int(1, 1), received.Value.Origin);
      Assert.AreEqual(HighlightState.Valid, received.Value.State);
    }

    [Test]
    public void RequestHighlight_CanFireMultipleTimes()
    {
      int count = 0;
      _presenter.OnHighlightRequested.Subscribe(_ => count++);

      var cfg = MakeCfg();
      _presenter.RequestHighlight(cfg, Vector2Int.zero, HighlightState.Valid);
      _presenter.RequestHighlight(cfg, Vector2Int.zero, HighlightState.None);
      _presenter.RequestHighlight(cfg, Vector2Int.zero, HighlightState.Merge);

      Assert.AreEqual(3, count);
    }

    #endregion

    #region Helpers

    private static ItemConfig MakeCfg()
    {
      var cfg = ScriptableObject.CreateInstance<ItemConfig>();
      typeof(ItemConfig)
        .GetField("<Shape>k__BackingField",
          System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
        ?.SetValue(cfg, new List<Vector2Int> { Vector2Int.zero });
      return cfg;
    }

    private static InventoryItem MakeItem() =>
      new InventoryItem(MakeCfg(), Vector2Int.zero);
  }
  #endregion
}
