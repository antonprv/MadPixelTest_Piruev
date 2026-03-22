// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using UnityEngine;

namespace Code.Infrastructure.Services.StaticData.Interfaces
{
  public interface IBagConfigSubservice
  {
    Vector2Int GridSize       { get; }
    int        BottomSlotCount { get; }
    float      CellSize       { get; }
    float      CellSpacing    { get; }

    /// <summary>
    /// Returns the set of active grid cells.
    /// If BagConfig has no custom shape, returns all cells in the GridSize rectangle.
    /// </summary>
    HashSet<Vector2Int> GetActiveCellsSet();

    UniTask LoadSelfAsync();
  }
}
