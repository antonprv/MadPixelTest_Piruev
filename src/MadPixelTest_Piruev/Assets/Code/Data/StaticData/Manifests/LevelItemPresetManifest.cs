// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Common.CustomTypes.Domain.Collections;
using Code.Data.StaticData;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Code.Data.StaticData.Manifests
{
  /// <summary>
  /// Manifest that maps level names to LevelItemPreset ScriptableObjects.
  ///
  /// Key   — level name string (same key space as LevelBagManifest).
  /// Value — Addressable reference to the LevelItemPreset for that level.
  ///
  /// A level without an entry in this manifest starts with no items (empty bag).
  /// </summary>
  [CreateAssetMenu(fileName = "LevelItemPresetManifest",
                   menuName  = "StaticData/Manifests/Level Item Preset Manifest")]
  public class LevelItemPresetManifest : ScriptableObject
  {
    public DictionaryData<string, AssetReferenceT<LevelItemPreset>> Levels = new();
  }
}
