// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

using Code.Data.StaticData;
using Code.Infrastructure.AssetManagement;
using Code.Infrastructure.AssetManagement.Addresses;
using Code.Infrastructure.Services.StaticData.Interfaces;

using Cysharp.Threading.Tasks;

using UnityEngine;

namespace Code.Infrastructure.Services.StaticData.Subservices
{
  /// <summary>
  /// Loads BagConfig from Addressables and exposes its values as flat properties.
  /// Consumers (GridInventoryService, BottomSlotsService, BagViewModel, BottomSlotsViewModel)
  /// depend on IBagConfigSubservice — they never touch BagConfig directly.
  /// </summary>
  public class BagConfigSubservice : IBagConfigSubservice
  {
    public Vector2Int GridSize        { get; private set; }
    public int        BottomSlotCount { get; private set; }
    public float      CellSize        { get; private set; }
    public float      CellSpacing     { get; private set; }

    private BagConfig _config;

    private readonly IAssetLoader _assetLoader;

    public BagConfigSubservice(IAssetLoader assetLoader) =>
      _assetLoader = assetLoader;

    public async UniTask LoadSelfAsync()
    {
      if (_config != null) return;

      _config = await _assetLoader.LoadAsync<BagConfig>(StaticDataAddresses.BagConfig);

      GridSize        = _config.GridSize;
      BottomSlotCount = _config.BottomSlotCount;
      CellSize        = _config.CellSize;
      CellSpacing     = _config.CellSpacing;
    }

    /// <inheritdoc/>
    public HashSet<Vector2Int> GetActiveCellsSet() => _config.GetActiveCellsSet();
  }
}
