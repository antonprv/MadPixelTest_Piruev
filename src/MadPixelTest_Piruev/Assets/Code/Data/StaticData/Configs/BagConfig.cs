// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

using UnityEngine;

namespace Code.Data.StaticData.Configs
{
  /// <summary>
  /// ScriptableObject — bag config.
  /// ActiveCells: if empty — GridSize rectangle is used.
  /// If filled — only listed cells are active (non-standard shape).
  /// </summary>
  [CreateAssetMenu(fileName = "BagConfig", menuName = "StaticData/Configs/Bag Config")]
  public class BagConfig : ScriptableObject
  {
    [field: SerializeField] public Vector2Int GridSize { get; private set; } = new(5, 7);
    [field: SerializeField] public int BottomSlotCount { get; private set; } = 5;
    [field: SerializeField] public float CellSize { get; private set; } = 80f;
    [field: SerializeField] public float CellSpacing { get; private set; } = 4f;

    [Tooltip("Leave empty — GridSize rectangle will be used. " +
             "For non-standard shape, list active cells.")]
    [SerializeField] private List<Vector2Int> _activeCells = new();

    public bool UseCustomShape => _activeCells != null && _activeCells.Count > 0;

    /// <summary>Returns HashSet of active cells (ready for use in GridInventory).</summary>
    public HashSet<Vector2Int> GetActiveCellsSet()
    {
      var set = new HashSet<Vector2Int>();

      if (UseCustomShape)
      {
        foreach (var cell in _activeCells)
          set.Add(cell);
      }
      else
      {
        for (int x = 0; x < GridSize.x; x++)
          for (int y = 0; y < GridSize.y; y++)
            set.Add(new Vector2Int(x, y));
      }

      return set;
    }
  }
}
