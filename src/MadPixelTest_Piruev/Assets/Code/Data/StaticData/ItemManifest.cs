// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

using UnityEngine;

namespace Code.Data.StaticData
{
  /// <summary>
  /// Manifest of all ItemConfigs in the game.
  /// AssetsPreloader reads this list and warms up all item icons
  /// before gameplay starts, so first render is instant.
  ///
  /// Adding a new item: create ItemConfig SO → add it here.
  /// No code changes needed.
  /// </summary>
  [CreateAssetMenu(fileName = "ItemManifest", menuName = "StaticData/Item Manifest")]
  public class ItemManifest : ScriptableObject
  {
    [field: SerializeField]
    public List<ItemConfig> Items { get; private set; } = new();
  }
}
