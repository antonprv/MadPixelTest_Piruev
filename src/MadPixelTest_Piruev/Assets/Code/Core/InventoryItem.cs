using System.Collections.Generic;
using UnityEngine;
using BagFight.Data;

namespace BagFight.Core
{
  /// <summary>
  /// Рантаймовый экземпляр предмета на гриде.
  /// Хранит конфиг + текущий origin (верхний-левый угол bounding box).
  /// </summary>
  public class InventoryItem
  {
    public ItemConfig  Config { get; }
    public Vector2Int  Origin { get; private set; }

    public InventoryItem(ItemConfig config, Vector2Int origin)
    {
      Config = config;
      Origin = origin;
    }

    /// <summary>Обновляет позицию (используется при возврате на место после неудачного дропа).</summary>
    public void SetOrigin(Vector2Int origin) => Origin = origin;

    /// <summary>Перечисляет все клетки, которые занимает предмет на гриде.</summary>
    public IEnumerable<Vector2Int> GetOccupiedCells() => Config.GetOccupiedCells(Origin);
  }
}
