// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Code.ViewModel.DragIcon;

using NUnit.Framework;

using R3;

using UnityEngine;

namespace Code.Tests.EditMode.ViewModel
{
  #region DragIconViewModel

  [TestFixture]
  public class DragIconViewModelTests
  {
    private DragIconViewModel _vm;

    [SetUp]
    public void SetUp() => _vm = new DragIconViewModel();

    [Test]
    public void InitialState_IsHidden()
    {
      Assert.IsFalse(_vm.IsVisible.CurrentValue);
      Assert.IsNull(_vm.Sprite.CurrentValue);
    }

    [Test]
    public void Show_SetsVisibleAndSprite()
    {
      var sprite = CreateMockSprite();
      _vm.Show(sprite, new Vector2(10f, 20f));

      Assert.IsTrue(_vm.IsVisible.CurrentValue);
      Assert.AreSame(sprite, _vm.Sprite.CurrentValue);
    }

    [Test]
    public void Show_FiresPositionUpdate()
    {
      Vector2? received = null;
      _vm.OnPositionUpdate.Subscribe(p => received = p);

      _vm.Show(null, new Vector2(50f, 60f));

      Assert.IsNotNull(received);
      Assert.AreEqual(new Vector2(50f, 60f), received.Value);
    }

    [Test]
    public void UpdatePosition_FiresPositionUpdate()
    {
      Vector2? received = null;
      _vm.OnPositionUpdate.Subscribe(p => received = p);

      _vm.UpdatePosition(new Vector2(100f, 200f));

      Assert.AreEqual(new Vector2(100f, 200f), received.Value);
    }

    [Test]
    public void Hide_ClearsVisibilityAndSprite()
    {
      _vm.Show(null, Vector2.zero);
      _vm.Hide();

      Assert.IsFalse(_vm.IsVisible.CurrentValue);
      Assert.IsNull(_vm.Sprite.CurrentValue);
    }

    [Test]
    public void Show_Then_Hide_ThenShow_WorksCorrectly()
    {
      var sprite = CreateMockSprite();
      _vm.Show(sprite, Vector2.zero);
      _vm.Hide();
      _vm.Show(sprite, Vector2.zero);

      Assert.IsTrue(_vm.IsVisible.CurrentValue);
    }

    [Test]
    public void OnPositionUpdate_FiresMultipleTimes()
    {
      int count = 0;
      _vm.OnPositionUpdate.Subscribe(_ => count++);

      _vm.UpdatePosition(Vector2.zero);
      _vm.UpdatePosition(Vector2.one);
      _vm.UpdatePosition(new Vector2(5, 5));

      Assert.AreEqual(3, count);
    }

    #region Helpers
    private static Sprite CreateMockSprite()
    {
      var tex = new Texture2D(1, 1);
      var sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.zero);
      return sprite;
    }

    #endregion
  }

  #endregion
}
