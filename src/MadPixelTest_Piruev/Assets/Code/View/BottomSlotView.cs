// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Code.ViewModel.BottomSlot;

using R3;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using Zenjex.Extensions.Injector;

namespace Code.View
{
  public class BottomSlotView : ZenjexBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IDropHandler
  {
    [Header("Visual")]
    [SerializeField] private Image _background;
    [SerializeField] private Image _iconImage;

    [Header("Animation")]
    [SerializeField] private float _bounceDuration = 0.15f;

    private IBottomSlotViewModel _viewModel;
    private CompositeDisposable  _disposables;

    public void SetViewModel(IBottomSlotViewModel viewModel)
    {
      _viewModel = viewModel;
      _disposables?.Dispose();
      _disposables = new CompositeDisposable();
      Bind();
    }

    private void OnDestroy() => _disposables?.Dispose();

    private void Bind()
    {
      _viewModel.BackgroundColor
        .Subscribe(c => _background.color = c)
        .AddTo(_disposables);

      _viewModel.Icon
        .Subscribe(sprite =>
        {
          if (_iconImage == null) return;
          _iconImage.sprite  = sprite;
          _iconImage.enabled = sprite != null;
        })
        .AddTo(_disposables);
    }

    #region Input

    public void OnBeginDrag(PointerEventData e) => _viewModel?.OnBeginDrag(e.position);
    public void OnDrag(PointerEventData e)      => _viewModel?.OnDrag(e.position);
    public void OnEndDrag(PointerEventData e)   => _viewModel?.OnEndDrag();

    public void OnDrop(PointerEventData e)
    {
      _viewModel?.OnDrop();
      // Bounce is also triggered reactively via BottomSlotsView → IsEmpty subscription,
      // but calling it here too gives instant feedback on direct OnDrop
      PlayBounce();
    }

    #endregion

    /// <summary>
    /// Public so BottomSlotsView can trigger it reactively when an item
    /// returns to this slot via cancel/fly-back.
    /// </summary>
    public void PlayBounce()
    {
      LeanTween.cancel(gameObject);
      transform.localScale = Vector3.one * 0.85f;
      LeanTween.scale(gameObject, Vector3.one, _bounceDuration).setEaseOutBack();
    }
  }
}
