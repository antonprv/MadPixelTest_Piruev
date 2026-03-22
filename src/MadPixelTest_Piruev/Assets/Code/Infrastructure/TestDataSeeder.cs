// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

using Code.UI.Services.BottomSlots.Interfaces;

using Code.Core;
using Code.Data.StaticData;

using UnityEngine;

using Zenjex.Extensions.Attribute;
using Zenjex.Extensions.Injector;

namespace Code.Infrastructure
{
  /// <summary>
  /// Fills bottom slots with test items on startup.
  /// Remove or disable this component before submission if not needed.
  ///
  /// Usage:
  ///   1. Add this component to any GameObject in the scene
  ///   2. In the inspector, drag ItemConfigs into the StartingItems list
  ///   3. On Play, bottom slots will be filled with items in order
  /// </summary>
  public class TestDataSeeder : ZenjexBehaviour
  {
    [SerializeField] private List<ItemConfig> _startingItems = new();

    [Zenjex] private IBottomSlotsService _slotsService;

    protected override void OnAwake()
    {
      for (int i = 0; i < _startingItems.Count; i++)
      {
        var config = _startingItems[i];
        if (config == null) continue;

        // origin doesn't matter for slots — set to zero,
        // actual origin is assigned when placed in bag
        var item = new InventoryItem(config, Vector2Int.zero);

        if (!_slotsService.TryPlace(item, i))
          Debug.LogWarning($"[TestDataSeeder] Slot {i} already occupied or out of range.");
      }
    }
  }
}
