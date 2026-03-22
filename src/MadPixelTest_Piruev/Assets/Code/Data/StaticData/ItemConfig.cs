// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Code.Data.StaticData
{
  /// <summary>
  /// ScriptableObject — config for one item type.
  /// Shape — list of offsets from origin (top-left corner of bounding box).
  /// Example L-shape: [(0,0), (0,1), (0,2), (1,2)]
  /// MergeResult — what item two identical ones become on merge.
  ///
  /// Icon — AssetReferenceSprite (Addressable).
  /// AssetsPreloader warms up all icons by label "ItemIcon" before gameplay starts,
  /// so at runtime AssetLoader.LoadAsync returns result from cache instantly.
  /// </summary>
  [CreateAssetMenu(fileName = "ItemConfig", menuName = "StaticData/Item Config")]
  public class ItemConfig : ScriptableObject
  {
    [field: SerializeField] public string ItemId { get; private set; }
    [field: SerializeField] public int Level { get; private set; } = 1;
    [field: SerializeField] public AssetReferenceSprite Icon { get; private set; }
    [field: SerializeField] public Color ItemColor { get; private set; } = Color.white;

    [field: SerializeField]
    public List<Vector2Int> Shape { get; private set; } = new() { Vector2Int.zero };

    /// <summary>What results from merging two identical items. null — merge disabled.</summary>
    [field: SerializeField] public ItemConfig MergeResult { get; private set; }

    public bool CanMerge => MergeResult != null;

    /// <summary>Bounding box size in cells (width × height).</summary>
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

    /// <summary>Returns all cells occupied by item when placed at origin.</summary>
    public IEnumerable<Vector2Int> GetOccupiedCells(Vector2Int origin)
    {
      foreach (var offset in Shape)
        yield return origin + offset;
    }
  }
}
