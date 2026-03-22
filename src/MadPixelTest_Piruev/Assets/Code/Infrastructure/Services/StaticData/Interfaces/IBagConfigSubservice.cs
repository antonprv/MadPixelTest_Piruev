// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using UnityEngine;

namespace Code.Infrastructure.Services.StaticData.Interfaces
{
  /// <summary>
  /// Exposes bag layout properties consumed by domain services and ViewModels.
  /// Backed by LevelBagConfigSubservice which delegates live to
  /// ILevelStaticDataService.CurrentBagConfig after LoadForLevelAsync().
  /// LoadSelfAsync() is a no-op — the real load is done per-level in LoadLevelState.
  /// </summary>
  public interface IBagConfigSubservice
  {
    Vector2Int GridSize        { get; }
    int        BottomSlotCount { get; }
    float      CellSize        { get; }
    float      CellSpacing     { get; }

    HashSet<Vector2Int> GetActiveCellsSet();

    UniTask LoadSelfAsync();
  }
}
