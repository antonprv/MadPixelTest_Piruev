// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

namespace Code.Infrastructure.AssetManagement
{
  /// <summary>
  /// String addresses for Addressable UI assets.
  /// Values must match the "Address" field in Addressables Groups window.
  /// </summary>
  public static class BagAssetAddresses
  {
    #region Root

    /// <summary>
    /// Empty GameObject with a Canvas — parent for all gameplay UI.
    /// Spawned once per level load, destroyed when returning to menu.
    /// </summary>
    public const string UIRootAddress = "UI/PUI_Root.prefab";

    #endregion

    #region Gameplay UI

    public const string BagCanvasAddress = "PUI_BagCanvas";

    /// <summary>
    /// HUD prefab — contains the "Return to Menu" button (HudView).
    /// Spawned as a child of UIRoot alongside BagCanvas.
    /// </summary>
    public const string HudAddress = "PUI_Hud";

    #endregion

    #region Preload label

    // All item icons are tagged with "Preload_UI" label in Addressables Groups.
    public const string ItemIconLabel = "Preload_UI";

    #endregion
  }
}
