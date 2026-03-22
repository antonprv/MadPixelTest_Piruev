using System.Collections.Generic;
using UnityEngine;
using BagFight.Core;
using BagFight.Data;
using BagFight.Services.Interfaces;
using Zenjex.Extensions.Attribute;
using Zenjex.Extensions.Injector;

namespace BagFight.Infrastructure
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
