using System.Collections.Generic;
using UnityEngine;

namespace BagFight.Data
{
  /// <summary>
  /// ScriptableObject — конфиг сумки.
  /// ActiveCells: если пустой — используется прямоугольник GridSize.
  /// Если заполнен — только перечисленные клетки активны (нестандартная форма).
  /// </summary>
  [CreateAssetMenu(fileName = "BagConfig", menuName = "BagFight/Bag Config")]
  public class BagConfig : ScriptableObject
  {
    [field: SerializeField] public Vector2Int GridSize       { get; private set; } = new(5, 7);
    [field: SerializeField] public int        BottomSlotCount { get; private set; } = 5;
    [field: SerializeField] public float      CellSize       { get; private set; } = 80f;
    [field: SerializeField] public float      CellSpacing    { get; private set; } = 4f;

    [Tooltip("Оставь пустым — будет использован весь прямоугольник GridSize. " +
             "Для нестандартной формы перечисли активные клетки.")]
    [SerializeField] private List<Vector2Int> _activeCells = new();

    public bool UseCustomShape => _activeCells != null && _activeCells.Count > 0;

    /// <summary>Возвращает HashSet активных клеток (готов к использованию в GridInventory).</summary>
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
