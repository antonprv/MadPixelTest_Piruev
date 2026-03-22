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

    // ── Initial state ──────────────────────────────────────────────────────

    [Test]
    public void InitialState_IsHidden()
    {
      Assert.IsFalse(_vm.IsVisible.CurrentValue);
      Assert.IsNull(_vm.Sprite.CurrentValue);
    }

    [Test]
    public void InitialState_DragItemBounds_IsZero()
    {
      Assert.AreEqual(Vector2Int.zero, _vm.DragItemBounds);
    }

    // ── Show ───────────────────────────────────────────────────────────────

    [Test]
    public void Show_SetsVisibleAndSprite()
    {
      var sprite = CreateSprite();
      var bounds = new Vector2Int(2, 3);

      _vm.Show(sprite, new Vector2(10f, 20f), bounds);

      Assert.IsTrue(_vm.IsVisible.CurrentValue);
      Assert.AreSame(sprite, _vm.Sprite.CurrentValue);
    }

    [Test]
    public void Show_StoresDragItemBounds()
    {
      var bounds = new Vector2Int(2, 3);
      _vm.Show(CreateSprite(), Vector2.zero, bounds);

      Assert.AreEqual(bounds, _vm.DragItemBounds);
    }

    [Test]
    public void Show_FiresPositionUpdate()
    {
      Vector2? received = null;
      _vm.OnPositionUpdate.Subscribe(p => received = p);

      _vm.Show(null, new Vector2(50f, 60f), Vector2Int.one);

      Assert.IsNotNull(received);
      Assert.AreEqual(new Vector2(50f, 60f), received.Value);
    }

    // ── UpdatePosition ─────────────────────────────────────────────────────

    [Test]
    public void UpdatePosition_FiresPositionUpdate()
    {
      Vector2? received = null;
      _vm.OnPositionUpdate.Subscribe(p => received = p);

      _vm.UpdatePosition(new Vector2(100f, 200f));

      Assert.AreEqual(new Vector2(100f, 200f), received.Value);
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

    // ── Hide ───────────────────────────────────────────────────────────────

    [Test]
    public void Hide_ClearsVisibilityAndSprite()
    {
      _vm.Show(null, Vector2.zero, Vector2Int.one);
      _vm.Hide();

      Assert.IsFalse(_vm.IsVisible.CurrentValue);
      Assert.IsNull(_vm.Sprite.CurrentValue);
    }

    // ── Show → Hide → Show ─────────────────────────────────────────────────

    [Test]
    public void Show_Then_Hide_ThenShow_WorksCorrectly()
    {
      var sprite = CreateSprite();
      _vm.Show(sprite, Vector2.zero, Vector2Int.one);
      _vm.Hide();
      _vm.Show(sprite, Vector2.zero, new Vector2Int(3, 1));

      Assert.IsTrue(_vm.IsVisible.CurrentValue);
      Assert.AreEqual(new Vector2Int(3, 1), _vm.DragItemBounds);
    }

    // ── FlyTo ──────────────────────────────────────────────────────────────

    [Test]
    public void FlyTo_FiresOnFlyTo_WithCorrectPosition()
    {
      Vector2? received = null;
      _vm.OnFlyTo.Subscribe(p => received = p);

      var target = new Vector2(300f, 150f);
      _vm.FlyTo(target);

      Assert.IsNotNull(received);
      Assert.AreEqual(target, received.Value);
    }

    [Test]
    public void FlyTo_DoesNotHideImmediately()
    {
      _vm.Show(CreateSprite(), Vector2.zero, Vector2Int.one);
      _vm.FlyTo(new Vector2(100f, 100f));

      // Hide is deferred to DragIconView after animation completes
      Assert.IsTrue(_vm.IsVisible.CurrentValue);
    }

    [Test]
    public void OnFlyTo_FiresMultipleTimes()
    {
      int count = 0;
      _vm.OnFlyTo.Subscribe(_ => count++);

      _vm.FlyTo(Vector2.zero);
      _vm.FlyTo(Vector2.one);

      Assert.AreEqual(2, count);
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static Sprite CreateSprite()
    {
      var tex = new Texture2D(1, 1);
      return Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.zero);
    }
  }

  #endregion
}
