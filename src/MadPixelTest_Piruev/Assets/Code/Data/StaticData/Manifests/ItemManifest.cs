// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Data.StaticData.Configs;

using Code.Common.CustomTypes.Domain.Collections;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Code.Data.StaticData.Manifests
{
  /// <summary>
  /// Manifest of all ItemConfigs in the game.
  ///
  /// Stores Addressable references keyed by ItemId (string).
  /// ItemDataSubservice loads this manifest, then resolves each reference
  /// on demand via IAssetLoader.
  ///
  /// Adding a new item:
  ///   1. Create an ItemConfig ScriptableObject asset.
  ///   2. Mark it as Addressable.
  ///   3. Open this manifest → click "AutoFill from Assets" in the inspector.
  ///      The editor will find all ItemConfig assets and fill the dictionary.
  ///   No code changes needed.
  /// </summary>
  [CreateAssetMenu(fileName = "ItemManifest", menuName = "StaticData/Manifests/Item Manifest")]
  public class ItemManifest : ScriptableObject
  {
    public DictionaryData<string, AssetReferenceT<ItemConfig>> Items = new();
  }
}
