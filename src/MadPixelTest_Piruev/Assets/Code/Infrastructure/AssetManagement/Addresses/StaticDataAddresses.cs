// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

namespace Code.Infrastructure.AssetManagement.Addresses
{
  /// <summary>
  /// String addresses for Addressable static-data assets.
  /// Values must match the "Address" field in the Addressables Groups window.
  /// </summary>
  public static class StaticDataAddresses
  {
    public const string BagConfig    = "BagConfig";
    public const string ItemManifest = "ItemManifest";

    /// <summary>Level name → BagConfig manifest.</summary>
    public const string LevelBagManifest        = "LevelBagManifest";

    /// <summary>Level name → LevelItemPreset manifest.</summary>
    public const string LevelItemPresetManifest = "LevelItemPresetManifest";
  }
}
