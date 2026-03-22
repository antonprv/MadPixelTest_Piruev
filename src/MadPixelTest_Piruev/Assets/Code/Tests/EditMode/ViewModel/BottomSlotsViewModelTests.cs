// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Infrastructure.AssetManagement;
using Code.Infrastructure.Services.StaticData.Interfaces;
using Code.Model.Core;
using Code.Presenter.BottomSlots;
using Code.Presenter.DragDrop;
using Code.ViewModel.BottomSlots;

using NSubstitute;

using NUnit.Framework;

using R3;

using UnityEngine;

namespace Code.Tests.EditMode.ViewModel
{
  #region BottomSlotsViewModel

  [TestFixture]
  public class BottomSlotsViewModelTests
  {
    private IBottomSlotsPresenter _slotsPresenter;
    private IDragDropPresenter    _dragDropPresenter;
    private IAssetLoader          _assetLoader;
    private BottomSlotsViewModel  _vm;
    private const int SlotCount = 5;

    [SetUp]
    public void SetUp()
    {
      _slotsPresenter    = Substitute.For<IBottomSlotsPresenter>();
      _dragDropPresenter = Substitute.For<IDragDropPresenter>();
      _assetLoader       = Substitute.For<IAssetLoader>();

      _slotsPresenter.OnSlotChanged.Returns(Observable.Empty<int>());
      _slotsPresenter.GetSlot(Arg.Any<int>()).Returns((InventoryItem)null);

      var bagConfig = MakeBagConfig(SlotCount);
      _vm = new BottomSlotsViewModel(bagConfig, _slotsPresenter, _dragDropPresenter, _assetLoader);
    }

    [TearDown]
    public void TearDown() => _vm.Dispose();

    [Test]
    public void SlotCount_MatchesBagConfig() =>
      Assert.AreEqual(SlotCount, _vm.SlotCount);

    [Test]
    public void GetSlotViewModel_ValidIndex_ReturnsNonNull()
    {
      for (int i = 0; i < SlotCount; i++)
        Assert.IsNotNull(_vm.GetSlotViewModel(i));
    }

    [Test]
    public void GetSlotViewModel_EachIndex_ReturnsDifferentViewModel()
    {
      var vm0 = _vm.GetSlotViewModel(0);
      var vm1 = _vm.GetSlotViewModel(1);
      Assert.AreNotSame(vm0, vm1);
    }

    [Test]
    public void Dispose_DoesNotThrow() =>
      Assert.DoesNotThrow(() => _vm.Dispose());

    #endregion

    #region Helpers

    /// <summary>
    /// Returns a mock IBagConfigSubservice with the given bottom slot count.
    /// Avoids any dependency on the BagConfig ScriptableObject in unit tests.
    /// </summary>
    private static IBagConfigSubservice MakeBagConfig(int bottomSlots)
    {
      var mock = Substitute.For<IBagConfigSubservice>();
      mock.BottomSlotCount.Returns(bottomSlots);
      return mock;
    }

    #endregion
  }
}
