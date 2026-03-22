using System.Collections.Generic;
using UnityEngine;

namespace BagFight.Data
{
  /// <summary>
  /// Манифест всех ItemConfig'ов в игре.
  /// AssetsPreloader читает этот список и прогревает иконки всех предметов
  /// до начала геймплея, чтобы первый рендер был мгновенным.
  ///
  /// Добавление нового предмета: создай ItemConfig SO → добавь его сюда.
  /// Код менять не нужно.
  /// </summary>
  [CreateAssetMenu(fileName = "ItemManifest", menuName = "BagFight/Item Manifest")]
  public class ItemManifest : ScriptableObject
  {
    [field: SerializeField]
    public List<ItemConfig> Items { get; private set; } = new();
  }
}
