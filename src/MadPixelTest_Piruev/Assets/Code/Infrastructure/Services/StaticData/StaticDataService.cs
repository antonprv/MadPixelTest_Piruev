// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Infrastructure.Services.StaticData.Interfaces;

using Cysharp.Threading.Tasks;

namespace Code.Infrastructure.Services.StaticData
{
  /// <summary>
  /// Root static-data service.
  ///
  /// Two independent sets of data:
  ///   1. BagConfig + ItemManifest — global, loaded once in PreloadAssetsState.
  ///   2. LevelData (LevelBagManifest + LevelItemPresetManifest) — manifests
  ///      loaded in PreloadAssetsState; per-level assets resolved in LoadLevelState.
  ///
  /// Domain services that need bag dimensions use IBagConfigSubservice (per-level, via LevelBagConfigSubservice).
  /// Domain services that need per-level data use ILevelStaticDataService.
  /// They are two separate services — no coupling between them.
  /// </summary>
  public class StaticDataService : IStaticDataService
  {
    public IBagConfigSubservice    BagConfig { get; }
    public IItemDataSubservice     ItemData  { get; }
    public ILevelStaticDataService LevelData { get; }

    public StaticDataService(
      IBagConfigSubservice    bagConfig,
      IItemDataSubservice     itemData,
      ILevelStaticDataService levelData)
    {
      BagConfig = bagConfig;
      ItemData  = itemData;
      LevelData = levelData;
    }

    public async UniTask LoadAllAsync() =>
      await UniTask.WhenAll(
        BagConfig.LoadSelfAsync(),
        ItemData.LoadSelfAsync(),
        LevelData.LoadManifestsAsync());
  }
}
