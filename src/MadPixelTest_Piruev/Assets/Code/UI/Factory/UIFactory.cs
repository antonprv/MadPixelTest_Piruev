// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Infrastructure.AssetManagement;
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

      // Resolve ViewModels lazily — domain services are initialized at this point
      var bagViewModel          = _container.Resolve<IBagViewModel>();
      var bottomSlotsViewModel  = _container.Resolve<IBottomSlotsViewModel>();
      var dragIconViewModel     = _container.Resolve<IDragIconViewModel>();

      // BagView needs assetLoader to load item icon sprites
      canvas.GetComponentInChildren<BagView>(includeInactive: true)
        .Construct(bagViewModel, _assetLoader);

      canvas.GetComponentInChildren<BottomSlotsView>(includeInactive: true)
        .Construct(bottomSlotsViewModel);

      canvas.GetComponentInChildren<DragIconView>(includeInactive: true)
        .Construct(dragIconViewModel);
    }
  }
}
