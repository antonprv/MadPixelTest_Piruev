// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

#if UNITY_EDITOR
using System.Collections.Generic;

using Code.Data.StaticData.Manifests;

using Code.Data.StaticData;
using Code.Editor.Common;
using Code.Editor.Common.Manifests;
using Code.Editor.Common.Manifests.Drawers;
using Code.Editor.Common.Manifests.Interfaces;

using UnityEditor;

using UnityEngine.AddressableAssets;

namespace Code.Editor.StaticData.Manifests
{
  /// <summary>
  /// Custom inspector for LevelItemPresetManifest.
  ///
  /// Key   — level name string rendered as a scene-name dropdown
  ///          (uses InspectorUtils.GetAllScenes() for autocomplete).
  /// Value — AssetReferenceT&lt;LevelItemPreset&gt;
  ///
  /// AutoFill scans all LevelItemPreset assets and adds them keyed by
  /// asset file name. Levels without an entry start with an empty inventory.
  /// </summary>
  [CustomEditor(typeof(LevelItemPresetManifest))]
  public class LevelItemPresetManifestEditor
    : ManifestEditorBase<LevelItemPresetManifest, LevelItemPreset, string>
  {
    protected override string GetDictionaryPropertyName() =>
      nameof(LevelItemPresetManifest.Levels);

    protected override string GetDictionaryDisplayLabel() =>
      "Levels  (Scene Name → LevelItemPreset)";

    protected override string GetKeyFromData(LevelItemPreset data) => data.name;

    protected override IDictionary<string, AssetReferenceT<LevelItemPreset>>
      GetDictionary(LevelItemPresetManifest manifest) => manifest.Levels;

    protected override ICustomKeyDrawer CreateCustomKeyDrawer() =>
      new SceneDropdownKeyDrawer("ItemPreset");

    protected override void DrawBeforeAutoFill()
    {
      EditorGUILayout.HelpBox(
        "Maps level scene names to LevelItemPreset assets.\n\n" +
        "AutoFill adds all LevelItemPreset assets found in the project.\n" +
        "Levels without an entry here start with an empty inventory.\n\n" +
        "Level names must match those in LevelBagManifest.",
        MessageType.Info);
    }
  }
}
#endif
