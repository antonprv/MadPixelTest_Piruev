// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

#if UNITY_EDITOR
using System.Collections.Generic;

using Code.Data.StaticData;
using Code.Editor.Common.Manifests;

using UnityEditor;

using UnityEngine.AddressableAssets;

namespace Code.Editor.StaticData.Manifests
{
  /// <summary>
  /// Custom inspector for ItemManifest.
  ///
  /// Inherits ManifestEditorBase which provides:
  ///   - "AutoFill from Assets" button — scans the project for all ItemConfig
  ///     ScriptableObject assets and populates the dictionary automatically.
  ///   - Manual Save workflow — changes are saved explicitly via the Save button.
  ///   - Undo/redo support.
  ///
  /// Keys are ItemId strings read directly from each ItemConfig asset.
  /// The standard PropertyField drawer is used for entries (no custom key drawer
  /// needed — ItemId is a plain string, no enum or scene dropdown required).
  /// </summary>
  [CustomEditor(typeof(ItemManifest))]
  public class ItemManifestEditor
    : ManifestEditorBase<ItemManifest, ItemConfig, string>
  {
    // ── ManifestEditorBase contract ───────────────────────────────────────────

    protected override string GetDictionaryPropertyName() =>
      nameof(ItemManifest.Items);

    protected override string GetDictionaryDisplayLabel() =>
      "Items  (ItemId → ItemConfig)";

    /// <summary>
    /// Key is the ItemId field on the ScriptableObject.
    /// If ItemId is empty, falls back to the asset file name so the manifest
    /// never gets a blank key.
    /// </summary>
    protected override string GetKeyFromData(ItemConfig data) =>
      string.IsNullOrEmpty(data.ItemId) ? data.name : data.ItemId;

    protected override IDictionary<string, AssetReferenceT<ItemConfig>>
      GetDictionary(ItemManifest manifest) => manifest.Items;

    // ── Optional: header above the AutoFill button ────────────────────────────

    protected override void DrawBeforeAutoFill()
    {
      EditorGUILayout.HelpBox(
        "AutoFill scans all ItemConfig assets in the project and adds them " +
        "to the manifest keyed by ItemId.\n" +
        "Existing entries are updated if the asset path changed; " +
        "entries whose asset is no longer found are left untouched.",
        MessageType.Info);
    }
  }
}
#endif
