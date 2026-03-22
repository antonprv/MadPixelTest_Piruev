// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

using Code.Data.StaticData;
using Code.Data.StaticData.Configs;
using Code.Data.StaticData.Manifests;
using Code.Infrastructure.AssetManagement;
using Code.Infrastructure.AssetManagement.Addresses;
using Code.Infrastructure.Services.StaticData.Interfaces;

using Cysharp.Threading.Tasks;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Code.Infrastructure.Services.StaticData.Subservices
{
  /// <summary>
  /// Owns LevelBagManifest and LevelItemPresetManifest.
  ///
  /// Flow:
  ///   PreloadAssetsState  → LoadManifestsAsync()
  ///   LoadLevelState      → LoadForLevelAsync(levelName)
  ///   GameLoopState uses  → CurrentBagConfig / CurrentItemPreset
  ///
  /// After loading LevelItemPreset we eagerly load every ItemConfig inside it.
  /// AssetReference.Asset is only populated after the asset is loaded through
  /// Addressables — StartupItemsService reads entry.Item.Asset and needs it non-null.
  /// </summary>
  public class LevelStaticDataService : ILevelStaticDataService
  {
    public BagConfig       CurrentBagConfig  { get; private set; }
    public LevelItemPreset CurrentItemPreset { get; private set; }

    private LevelBagManifest        _bagManifest;
    private LevelItemPresetManifest _presetManifest;

    private readonly IAssetLoader _assetLoader;

    public LevelStaticDataService(IAssetLoader assetLoader) =>
      _assetLoader = assetLoader;

    // ── Manifest loading ───────────────────────────────────────────────────

    public async UniTask LoadManifestsAsync()
    {
      if (_bagManifest != null) return;

      (_bagManifest, _presetManifest) = await UniTask.WhenAll(
        _assetLoader.LoadAsync<LevelBagManifest>(StaticDataAddresses.LevelBagManifest),
        _assetLoader.LoadAsync<LevelItemPresetManifest>(
          StaticDataAddresses.LevelItemPresetManifest));
    }

    // ── Per-level resolution ───────────────────────────────────────────────

    public async UniTask LoadForLevelAsync(string levelName)
    {
      CurrentBagConfig  = null;
      CurrentItemPreset = null;

      // BagConfig is required
      if (_bagManifest.Levels.TryGetValue(levelName, out var bagRef))
      {
        CurrentBagConfig = await _assetLoader.LoadAsync<BagConfig>(bagRef);
      }
      else
      {
        Debug.LogWarning(
          $"[LevelStaticDataService] No BagConfig found for level '{levelName}'. " +
          "Falling back to default BagConfig address.");

        CurrentBagConfig =
          await _assetLoader.LoadAsync<BagConfig>(StaticDataAddresses.BagConfig);
      }

      // Item preset is optional — missing entry means empty inventory
      if (_presetManifest.Levels.TryGetValue(levelName, out var presetRef))
      {
        CurrentItemPreset = await _assetLoader.LoadAsync<LevelItemPreset>(presetRef);

        // Eagerly load every ItemConfig so entry.Item.Asset is non-null
        // when StartupItemsService.PlaceStartupItems() runs.
        await LoadPresetItemConfigsAsync(CurrentItemPreset);
      }
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private async UniTask LoadPresetItemConfigsAsync(LevelItemPreset preset)
    {
      if (preset?.Items == null || preset.Items.Count == 0)
        return;

      var tasks = new List<UniTask>(preset.Items.Count);

      foreach (var entry in preset.Items)
      {
        if (entry?.Item != null)
          tasks.Add(LoadItemConfigAsync(entry.Item));
      }

      if (tasks.Count > 0)
        await UniTask.WhenAll(tasks);
    }

    private async UniTask LoadItemConfigAsync(AssetReferenceT<ItemConfig> reference)
    {
      await _assetLoader.LoadAsync<ItemConfig>(reference);
    }
  }
}
