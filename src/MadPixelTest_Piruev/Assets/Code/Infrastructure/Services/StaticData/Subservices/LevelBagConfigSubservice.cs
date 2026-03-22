// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

using Code.Infrastructure.Services.StaticData.Interfaces;

using Cysharp.Threading.Tasks;

using UnityEngine;

namespace Code.Infrastructure.Services.StaticData.Subservices
{
  /// <summary>
  /// The one and only implementation of IBagConfigSubservice.
  ///
  /// Reads directly from ILevelStaticDataService.CurrentBagConfig, which is
  /// populated by LoadForLevelAsync() in LoadLevelState — before
  /// InitializeModelServices() is called.
  ///
  /// Properties are NOT cached — they delegate live to CurrentBagConfig so
  /// they always reflect the currently loaded level without any refresh step.
  ///
  /// LoadSelfAsync() is a no-op: it exists only to satisfy the
  /// IStaticDataService.LoadAllAsync() WhenAll call in PreloadAssetsState.
  /// The actual data load happens per-level in LoadLevelState.
  /// </summary>
  public class LevelBagConfigSubservice : IBagConfigSubservice
  {
    private readonly ILevelStaticDataService _levelData;

    public LevelBagConfigSubservice(ILevelStaticDataService levelData) =>
      _levelData = levelData;

    // ── IBagConfigSubservice ──────────────────────────────────────────────

    public Vector2Int GridSize =>
      _levelData.CurrentBagConfig?.GridSize ?? Vector2Int.zero;

    public int BottomSlotCount =>
      _levelData.CurrentBagConfig?.BottomSlotCount ?? 0;

    public float CellSize =>
      _levelData.CurrentBagConfig?.CellSize ?? 80f;

    public float CellSpacing =>
      _levelData.CurrentBagConfig?.CellSpacing ?? 4f;

    public HashSet<Vector2Int> GetActiveCellsSet() =>
      _levelData.CurrentBagConfig?.GetActiveCellsSet() ?? new HashSet<Vector2Int>();

    /// <summary>No-op — data is loaded per-level by ILevelStaticDataService.</summary>
    public UniTask LoadSelfAsync() => UniTask.CompletedTask;
  }
}
