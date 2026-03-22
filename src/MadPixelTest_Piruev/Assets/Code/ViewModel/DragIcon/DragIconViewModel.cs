// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using R3;

using UnityEngine;

namespace Code.ViewModel.DragIcon
{
  public interface IDragIconViewModel
  {
    ReadOnlyReactiveProperty<bool> IsVisible { get; }
    ReadOnlyReactiveProperty<Sprite> Sprite { get; }

    /// <summary>Fires whenever position should update (View calculates canvas coords).</summary>
    Observable<Vector2> OnPositionUpdate { get; }

    void Show(Sprite sprite, Vector2 screenPosition);
    void UpdatePosition(Vector2 screenPosition);
    void Hide();
  }

  /// <summary>
  /// MVVM ViewModel for the floating drag icon.
  ///
  /// Owns visibility and sprite state as ReactiveProperties.
  /// Position is forwarded as an Observable<Vector2> so the View
  /// can do its own canvas-coordinate math (requires Unity API).
  ///
  /// Registered as AsSingle — one icon shared by all drag interactions.
  /// </summary>
  public class DragIconViewModel : IDragIconViewModel
  {
    private readonly ReactiveProperty<bool> _isVisible = new(false);
    private readonly ReactiveProperty<Sprite> _sprite = new(null);
    private readonly Subject<Vector2> _positionUpdate = new();

    public ReadOnlyReactiveProperty<bool> IsVisible => _isVisible;
    public ReadOnlyReactiveProperty<Sprite> Sprite => _sprite;
    public Observable<Vector2> OnPositionUpdate => _positionUpdate;

    public void Show(Sprite sprite, Vector2 screenPosition)
    {
      _sprite.Value = sprite;
      _isVisible.Value = true;
      _positionUpdate.OnNext(screenPosition);
    }

    public void UpdatePosition(Vector2 screenPosition)
      => _positionUpdate.OnNext(screenPosition);

    public void Hide()
    {
      _isVisible.Value = false;
      _sprite.Value = null;
    }
  }
}
