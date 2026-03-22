// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Data.StaticData.Configs;

using Code.Common.CustomTypes.Domain.Collections;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Code.Data.StaticData.Manifests
{
  /// <summary>
  /// Manifest that maps level names to BagConfig ScriptableObjects.
  ///
  /// Key   — level name string (matches scene name or any string identifier).
  /// Value — Addressable reference to the BagConfig for that level.
  ///
  /// Usage:
  ///   LevelStaticDataService resolves the correct IBagConfigSubservice
  ///   for the current level by looking up the level name in this manifest.
  ///
  /// Editor setup:
  ///   1. Create one or more BagConfig assets and mark them Addressable.
  ///   2. Open this manifest and click "AutoFill from Assets" — it scans all
  ///      BagConfig assets and adds them, or add entries manually via the
  ///      scene-dropdown key drawer.
  /// </summary>
  [CreateAssetMenu(fileName = "LevelBagManifest",
                   menuName  = "StaticData/Manifests/Level Bag Manifest")]
  public class LevelBagManifest : ScriptableObject
  {
    public DictionaryData<string, AssetReferenceT<BagConfig>> Levels = new();
  }
}
