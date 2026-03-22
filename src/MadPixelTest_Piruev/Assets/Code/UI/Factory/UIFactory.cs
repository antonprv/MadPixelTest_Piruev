// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Infrastructure.AssetManagement;
using Code.Infrastructure.Services;
using Code.UI.Hud;
using Code.View;
using Code.ViewModel.Bag;
using Code.ViewModel.BottomSlots;
using Code.ViewModel.DragIcon;

using Cysharp.Threading.Tasks;

using Reflex.Core;

using UnityEngine;

namespace Code.UI.Factory
{
  /// <summary>
  /// Creates and owns all gameplay UI.
  ///
  /// Object hierarchy created per level:
  ///   UI_Root  (empty GameObject, DontDestroyOnLoad = false)
  ///   ├── PUI_BagCanvas   (bag grid + slots + drag icon)
  ///   └── PUI_Hud         (return-to-menu button, future HUD elements)
  ///
  /// All prefabs are cached in WarmUp() so instantiation is instant.
  /// Cleanup() destroys UI_Root, which takes all children with it.
  /// </summary>
  public class UIFactory : IUIFactory
  {
    private readonly IAssetLoader _assetLoader;
    private readonly Container    _container;

    // Cached prefabs — loaded once in WarmUp
    private GameObject _uiRootPrefab;
    private GameObject _bagCanvasPrefab;
    private GameObject _hudPrefab;

    // Live instance — destroyed in Cleanup
    private GameObject       _uiRoot;

    // Transient ViewModels — resolved fresh each level, disposed in Cleanup
    private IBagViewModel         _bagViewModel;
    private IBottomSlotsViewModel _bottomSlotsViewModel;

    public UIFactory(IAssetLoader assetLoader, Container container)
    {
      _assetLoader = assetLoader;
      _container   = container;
    }

    // ── WarmUp ────────────────────────────────────────────────────────────

    public async UniTask WarmUp()
    {
      (_uiRootPrefab, _bagCanvasPrefab, _hudPrefab) = await UniTask.WhenAll(
        _assetLoader.LoadAsync<GameObject>(BagAssetAddresses.UIRootAddress),
        _assetLoader.LoadAsync<GameObject>(BagAssetAddresses.BagCanvasAddress),
        _assetLoader.LoadAsync<GameObject>(BagAssetAddresses.HudAddress));
    }

    // ── UIRoot ────────────────────────────────────────────────────────────

    public void CreateUIRoot()
    {
      _uiRoot = Object.Instantiate(_uiRootPrefab);
      _uiRoot.name = "UI_Root";
    }

    // ── BagCanvas ─────────────────────────────────────────────────────────

    public async UniTask CreateGameplayUIAsync()
    {
      var canvas = Object.Instantiate(_bagCanvasPrefab, _uiRoot.transform);

      _bagViewModel         = _container.Resolve<IBagViewModel>();
      _bottomSlotsViewModel = _container.Resolve<IBottomSlotsViewModel>();
      var dragIconViewModel = _container.Resolve<IDragIconViewModel>();

      var bagView = canvas.GetComponentInChildren<BagView>(includeInactive: true);
      bagView.Construct(_bagViewModel, _assetLoader);

      var bottomSlotsView = canvas.GetComponentInChildren<BottomSlotsView>(includeInactive: true);
      bottomSlotsView.Construct(_bottomSlotsViewModel);

      SlotPositionProviderLocator.Register(bottomSlotsView);

      float step    = bagView.CellStep;
      float spacing = bagView.CellSpacing;

      var dragIconView = canvas.GetComponentInChildren<DragIconView>(includeInactive: true);
      dragIconView.Construct(dragIconViewModel, step, spacing);

      await UniTask.CompletedTask;
    }

    // ── HUD ───────────────────────────────────────────────────────────────

    public async UniTask<HudView> CreateHudAsync()
    {
      var hud     = Object.Instantiate(_hudPrefab, _uiRoot.transform);
      var hudView = hud.GetComponent<HudView>();

      await UniTask.CompletedTask;
      return hudView;
    }

    // ── Cleanup ───────────────────────────────────────────────────────────

    public void Cleanup()
    {
      if (_uiRoot != null)
        Object.Destroy(_uiRoot);

      _uiRoot = null;
      SlotPositionProviderLocator.Register(null);

      // Dispose transient ViewModels — they hold R3 Subjects and CompositeDisposables
      (_bagViewModel as BagViewModel)?.Dispose();
      (_bottomSlotsViewModel as BottomSlotsViewModel)?.Dispose();
      _bagViewModel         = null;
      _bottomSlotsViewModel = null;
    }
  }
}
