// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System;
using System.Collections.Generic;

using Code.Data.StaticData.Configs;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Code.Data.StaticData
{
  /// <summary>
  /// Defines the starting item loadout for one level.
  ///
  /// Each entry specifies an ItemConfig (Addressable reference) and a count.
  /// StartupItemsService reads this and calls TryPlace() for each item.
  ///
  /// Example: sword × 1, shield × 1, potion × 3.
  /// </summary>
  [CreateAssetMenu(fileName = "LevelItemPreset",
                   menuName  = "StaticData/Presets/Level Item Preset")]
  public class LevelItemPreset : ScriptableObject
  {
    [Serializable]
    public class Entry
    {
      [Tooltip("Addressable reference to the ItemConfig.")]
      public AssetReferenceT<ItemConfig> Item;

      [Min(1)]
      [Tooltip("How many of this item to give at level start.")]
      public int Count = 1;
    }

    [Tooltip("Items given to the player at the start of the level, in order.")]
    public List<Entry> Items = new();
  }
}
