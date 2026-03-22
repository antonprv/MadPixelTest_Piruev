// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

namespace Code.Infrastructure.AssetManagement
{
  /// <summary>
  /// String addresses for Addressable assets.
  /// Values must match "Address" field in Addressables Groups Unity window.
  /// </summary>
  public static class BagAssetAddresses
  {
    #region UI

    public const string BagCanvasAddress = "PUI_BagCanvas";

    #endregion

    #region Items (label for batch loading)

    // All item icons are tagged with "ItemIcon" label in Addressables Groups.
    // AssetsPreloader loads them all in one call via this label.
    public const string ItemIconLabel = "Preload_UI";

    #endregion

  }
}
