// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

using Code.Core;
using Code.Data.StaticData;
using Code.Services.Interfaces;

using UnityEngine;

using Zenjex.Extensions.Attribute;
using Zenjex.Extensions.Injector;

namespace Code.Infrastructure
{
  /// <summary>
  /// Заполняет нижние слоты тестовыми предметами при старте.
  /// Удали или отключи этот компонент перед сдачей, если не нужен.
  ///
  /// Использование:
  ///   1. Добавь компонент на любой GameObject в сцене
  ///   2. В инспекторе перетащи ItemConfig'и в список StartingItems
  ///   3. При Play нижние слоты заполнятся предметами по порядку
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

        // origin не важен для слотов — задаём нулевой,
        // реальный origin назначается при размещении в сумке
        var item = new InventoryItem(config, Vector2Int.zero);

        if (!_slotsService.TryPlace(item, i))
          Debug.LogWarning($"[TestDataSeeder] Slot {i} already occupied or out of range.");
      }
    }
  }
}
