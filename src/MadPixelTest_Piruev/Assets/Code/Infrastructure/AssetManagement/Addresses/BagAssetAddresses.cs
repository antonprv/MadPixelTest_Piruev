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
    // ── UI ────────────────────────────────────────────────────────────────────
    public const string LoadingCurtain = "UI/LoadingCurtain";
    public const string BagCanvas = "UI/BagCanvas";

    // ── Items (label for batch loading) ───────────────────────────────────────
    // All item icons are tagged with "ItemIcon" label in Addressables Groups.
    // AssetsPreloader loads them all in one call via this label.
    public const string ItemIconLabel = "ItemIcon";
  }
}
