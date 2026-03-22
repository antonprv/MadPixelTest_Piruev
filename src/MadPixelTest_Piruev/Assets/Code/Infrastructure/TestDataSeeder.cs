// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

using Code.Data.StaticData;
using Code.Model.Core;
using Code.Presenter.BottomSlots;

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
  ///
  /// Uses IBottomSlotsPresenter (MVP layer) instead of the service directly.
  /// </summary>
  public class TestDataSeeder : ZenjexBehaviour
  {
    [SerializeField] private List<ItemConfig> _startingItems = new();

    [Zenjex] private IBottomSlotsPresenter _slotsPresenter;

    protected override void OnAwake()
    {
      for (int i = 0; i < _startingItems.Count; i++)
      {
        var config = _startingItems[i];
        if (config == null) continue;

        var item = new InventoryItem(config, Vector2Int.zero);

        if (!_slotsPresenter.TryPlace(item, i))
          Debug.LogWarning($"[TestDataSeeder] Slot {i} already occupied or out of range.");
      }
    }
  }
}
