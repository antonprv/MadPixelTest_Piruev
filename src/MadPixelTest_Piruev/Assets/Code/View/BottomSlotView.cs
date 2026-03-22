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
  /// <summary>
  /// MVVM View — single bottom slot.
  ///
  /// Binds to IBottomSlotViewModel:
  ///   - IsEmpty / BackgroundColor / Icon → UI components
  ///   - Input events → ViewModel commands
  ///
  /// ViewModel is assigned by BottomSlotsView via SetViewModel().
  /// </summary>
  public class BottomSlotView : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IDropHandler
  {
    [Header("Visual")]
    [SerializeField] private Image _background;
    [SerializeField] private Image _iconImage;

    [Header("Animation")]
    [SerializeField] private float _bounceDuration = 0.15f;

    private IBottomSlotViewModel _viewModel;
    private CompositeDisposable _disposables;

    #region Init (called by BottomSlotsView)

    public void SetViewModel(IBottomSlotViewModel viewModel)
    {
      _viewModel = viewModel;

      _disposables?.Dispose();
      _disposables = new CompositeDisposable();

      Bind();
    }

    #endregion

    private void OnDestroy() => _disposables?.Dispose();

    #region Binding

    private void Bind()
    {
      _viewModel.BackgroundColor
        .Subscribe(c => _background.color = c)
        .AddTo(_disposables);

      _viewModel.Icon
        .Subscribe(sprite =>
        {
          if (_iconImage == null) return;
          _iconImage.sprite = sprite;
          _iconImage.enabled = sprite != null;
        })
        .AddTo(_disposables);
    }

    #endregion

    #region Input → ViewModel commands

    public void OnBeginDrag(PointerEventData eventData) =>
      _viewModel?.OnBeginDrag(eventData.position);

    public void OnDrag(PointerEventData eventData) =>
      _viewModel?.OnDrag(eventData.position);

    public void OnEndDrag(PointerEventData eventData) =>
      _viewModel?.OnEndDrag();

    public void OnDrop(PointerEventData eventData)
    {
      _viewModel?.OnDrop();
      PlayBounce();
    }

    #endregion

    #region Animation (purely visual)

    private void PlayBounce()
    {
      transform.localScale = Vector3.one * 0.85f;
      LeanTween
        .scale(gameObject, Vector3.one, _bounceDuration)
        .setEaseOutBack();
    }

    #endregion
  }
}
