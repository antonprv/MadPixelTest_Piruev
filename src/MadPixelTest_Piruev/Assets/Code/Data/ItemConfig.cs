using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace BagFight.Data
{
  /// <summary>
  /// ScriptableObject — конфиг одного типа предмета.
  /// Shape — список смещений от origin (верхний-левый угол bounding box).
  /// Пример L-шки: [(0,0), (0,1), (0,2), (1,2)]
  /// MergeResult — в какой предмет превращаются два одинаковых при мерже.
  ///
  /// Icon — AssetReferenceSprite (Addressable).
  /// AssetsPreloader прогревает все иконки по лейблу "ItemIcon" до старта геймплея,
  /// поэтому в рантайме AssetLoader.LoadAsync возвращает результат из кэша мгновенно.
  /// </summary>
  [CreateAssetMenu(fileName = "ItemConfig", menuName = "BagFight/Item Config")]
  public class ItemConfig : ScriptableObject
  {
    [field: SerializeField] public string               ItemId    { get; private set; }
    [field: SerializeField] public int                  Level     { get; private set; } = 1;
    [field: SerializeField] public AssetReferenceSprite Icon      { get; private set; }
    [field: SerializeField] public Color                ItemColor { get; private set; } = Color.white;

    [field: SerializeField]
    public List<Vector2Int> Shape { get; private set; } = new() { Vector2Int.zero };

    /// <summary>Что получается при мерже двух одинаковых предметов. null — мерж запрещён.</summary>
    [field: SerializeField] public ItemConfig MergeResult { get; private set; }

    public bool CanMerge => MergeResult != null;

    /// <summary>Размер bounding box в клетках (ширина × высота).</summary>
    public Vector2Int GetBoundsSize()
    {
      if (Shape == null || Shape.Count == 0)
        return Vector2Int.one;

      int maxX = 0, maxY = 0;
      foreach (var offset in Shape)
      {
        if (offset.x > maxX) maxX = offset.x;
        if (offset.y > maxY) maxY = offset.y;
      }
      return new Vector2Int(maxX + 1, maxY + 1);
    }

    /// <summary>Возвращает все клетки, занятые предметом при размещении по origin.</summary>
    public IEnumerable<Vector2Int> GetOccupiedCells(Vector2Int origin)
    {
      foreach (var offset in Shape)
        yield return origin + offset;
    }
  }
}
