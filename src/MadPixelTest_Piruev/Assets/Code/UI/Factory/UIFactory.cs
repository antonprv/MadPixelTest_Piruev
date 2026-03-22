// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Infrastructure.AssetManagement;
using Code.Infrastructure.Services;
using Code.View;
using Code.ViewModel.Bag;
using Code.ViewModel.BottomSlots;
using Code.ViewModel.DragIcon;

using Cysharp.Threading.Tasks;

using Reflex.Core;

using UnityEngine;

namespace Code.UI.Factory
{
  public class UIFactory : IUIFactory
  {
    private readonly IAssetLoader _assetLoader;
    private readonly Container    _container;

    public UIFactory(IAssetLoader assetLoader, Container container)
    {
      _assetLoader = assetLoader;
      _container   = container;
    }

    public async UniTask CreateGameplayUIAsync()
    {
      var prefab = await _assetLoader.LoadAsync<GameObject>(BagAssetAddresses.BagCanvasAddress);
      var canvas = Object.Instantiate(prefab);

      var bagViewModel         = _container.Resolve<IBagViewModel>();
      var bottomSlotsViewModel = _container.Resolve<IBottomSlotsViewModel>();
      var dragIconViewModel    = _container.Resolve<IDragIconViewModel>();

      // BagView needs assetLoader to load item icons
      var bagView = canvas.GetComponentInChildren<BagView>(includeInactive: true);
      bagView.Construct(bagViewModel, _assetLoader);

      // BottomSlotsView also acts as ISlotScreenPositionProvider —
      // register it in the container so DragDropPresenter can resolve it
      var bottomSlotsView = canvas.GetComponentInChildren<BottomSlotsView>(includeInactive: true);
      bottomSlotsView.Construct(bottomSlotsViewModel);

      // Register the position provider so DragDropPresenter can use it
      // (late binding after container was built — use RootContext if available,
      //  otherwise store on a static/singleton helper)
      SlotPositionProviderLocator.Register(bottomSlotsView);

      // DragIconView needs grid metrics to size the drag icon at ½ footprint
      // Read them from the already-constructed BagView
      float step    = bagView.CellStep;
      float spacing = bagView.CellSpacing;

      var dragIconView = canvas.GetComponentInChildren<DragIconView>(includeInactive: true);
      dragIconView.Construct(dragIconViewModel, step, spacing);
    }
  }
}
