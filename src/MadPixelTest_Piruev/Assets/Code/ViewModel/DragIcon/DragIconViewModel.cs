// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using R3;

using UnityEngine;

namespace Code.ViewModel.DragIcon
{
  public interface IDragIconViewModel
  {
    ReadOnlyReactiveProperty<bool>   IsVisible { get; }
    ReadOnlyReactiveProperty<Sprite> Sprite    { get; }

    /// <summary>Fires on every pointer move — View converts to canvas coords.</summary>
    Observable<Vector2> OnPositionUpdate { get; }

    /// <summary>
    /// Fires once when the drag ends with a "return" result (void drop or cancel).
    /// Carries the target screen position the icon should fly to.
    /// After the flight the icon hides itself.
    /// </summary>
    Observable<Vector2> OnFlyTo { get; }

    /// <summary>
    /// Bounding-box size of the dragged item in grid cells.
    /// DragIconView uses this to size the drag icon at ½ of its grid footprint.
    /// </summary>
    Vector2Int DragItemBounds { get; }

    void Show(Sprite sprite, Vector2 screenPosition, Vector2Int itemBounds);
    void UpdatePosition(Vector2 screenPosition);
    void Hide();

    /// <summary>
    /// Animate the icon flying to <paramref name="targetScreenPosition"/>,
    /// then hide it. Used when a dragged item is returned to a slot.
    /// </summary>
    void FlyTo(Vector2 targetScreenPosition);
  }

  public class DragIconViewModel : IDragIconViewModel
  {
    private readonly ReactiveProperty<bool>   _isVisible    = new(false);
    private readonly ReactiveProperty<Sprite> _sprite       = new(null);
    private readonly Subject<Vector2>         _positionUpdate = new();
    private readonly Subject<Vector2>         _flyTo          = new();

    public ReadOnlyReactiveProperty<bool>   IsVisible       => _isVisible;
    public ReadOnlyReactiveProperty<Sprite> Sprite          => _sprite;
    public Observable<Vector2>              OnPositionUpdate => _positionUpdate;
    public Observable<Vector2>              OnFlyTo          => _flyTo;

    public Vector2Int DragItemBounds { get; private set; }

    public void Show(Sprite sprite, Vector2 screenPosition, Vector2Int itemBounds)
    {
      DragItemBounds   = itemBounds;
      _sprite.Value    = sprite;
      _isVisible.Value = true;
      _positionUpdate.OnNext(screenPosition);
    }

    public void UpdatePosition(Vector2 screenPosition) =>
      _positionUpdate.OnNext(screenPosition);

    public void Hide()
    {
      _isVisible.Value = false;
      _sprite.Value    = null;
    }

    public void FlyTo(Vector2 targetScreenPosition)
    {
      // Signal the View to animate — View will call Hide() after the tween
      _flyTo.OnNext(targetScreenPosition);
    }
  }
}
