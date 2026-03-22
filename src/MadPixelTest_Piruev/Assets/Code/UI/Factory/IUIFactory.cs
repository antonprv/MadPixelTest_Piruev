// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Code.UI.Hud;

using Cysharp.Threading.Tasks;

namespace Code.UI.Factory
{
  /// <summary>
  /// Responsible for creating and wiring all gameplay UI.
  ///
  /// Lifecycle per level:
  ///   1. WarmUp()               — load prefabs into Addressables cache
  ///   2. CreateUIRoot()         — spawn empty UI_Root parent GameObject
  ///   3. CreateGameplayUIAsync()— spawn BagCanvas as child of UIRoot, wire ViewModels
  ///   4. CreateHudAsync()       — spawn HUD as child of UIRoot, wire button callbacks
  ///   5. Cleanup()              — destroy UIRoot (and all children) on return to menu
  /// </summary>
  public interface IUIFactory
  {
    /// <summary>
    /// Pre-loads UIRoot, BagCanvas and HUD prefabs into the Addressables cache.
    /// Call once before the first level load (or before each load to be safe).
    /// </summary>
    UniTask WarmUp();

    /// <summary>Spawns the empty UIRoot parent. Must be called before other Create* methods.</summary>
    void CreateUIRoot();

    /// <summary>Spawns BagCanvas as a child of UIRoot and wires all ViewModels.</summary>
    UniTask CreateGameplayUIAsync();

    /// <summary>
    /// Spawns HUD prefab as a child of UIRoot.
    /// Returns the HudView so GameLoopState can subscribe to its buttons.
    /// </summary>
    UniTask<HudView> CreateHudAsync();

    /// <summary>Destroys UIRoot and all its children. Call when leaving gameplay.</summary>
    void Cleanup();
  }
}
