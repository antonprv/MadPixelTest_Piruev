// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Data.StaticData.Configs;

using Code.Data.StaticData;

using Cysharp.Threading.Tasks;

namespace Code.Infrastructure.Services.StaticData.Interfaces
{
  /// <summary>
  /// Loads manifests (LevelBagManifest and LevelItemPresetManifest) once,
  /// then resolves the correct BagConfig and LevelItemPreset for a given level.
  ///
  /// Call LoadManifestsAsync() in PreloadAssetsState (alongside other static data).
  /// Call LoadForLevelAsync(levelName) in LoadLevelState before initialising
  /// domain services — this populates CurrentBagConfig and CurrentItemPreset.
  /// </summary>
  public interface ILevelStaticDataService
  {
    /// <summary>
    /// BagConfig resolved for the current level.
    /// Null until LoadForLevelAsync has completed.
    /// </summary>
    BagConfig CurrentBagConfig { get; }

    /// <summary>
    /// Item preset for the current level.
    /// Null if the level has no entry in the manifest (no startup items).
    /// </summary>
    LevelItemPreset CurrentItemPreset { get; }

    /// <summary>
    /// Loads both manifests from Addressables.
    /// Safe to call multiple times — subsequent calls are no-ops.
    /// </summary>
    UniTask LoadManifestsAsync();

    /// <summary>
    /// Resolves the BagConfig and LevelItemPreset for <paramref name="levelName"/>
    /// from the already-loaded manifests.
    /// Must be called after LoadManifestsAsync.
    /// </summary>
    UniTask LoadForLevelAsync(string levelName);
  }
}
