// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

#if UNITY_EDITOR
using System.Collections.Generic;

using Code.Data.StaticData.Configs;
using Code.Data.StaticData.Manifests;

using Code.Editor.Common;
using Code.Editor.Common.Manifests;
using Code.Editor.Common.Manifests.Drawers;
using Code.Editor.Common.Manifests.Interfaces;

using UnityEditor;

using UnityEngine.AddressableAssets;

namespace Code.Editor.StaticData.Manifests
{
  /// <summary>
  /// Custom inspector for LevelBagManifest.
  ///
  /// Key   — level name string rendered as a scene-name dropdown
  ///          (uses InspectorUtils.GetAllScenes() for autocomplete).
  /// Value — AssetReferenceT&lt;BagConfig&gt;
  ///
  /// AutoFill scans all BagConfig assets in the project and adds them,
  /// keyed by the asset file name (e.g. "Level_01_BagConfig" → "Level_01_BagConfig").
  /// You can then pick the correct scene from the dropdown for each entry.
  /// </summary>
  [CustomEditor(typeof(LevelBagManifest))]
  public class LevelBagManifestEditor
    : ManifestEditorBase<LevelBagManifest, BagConfig, string>
  {
    protected override string GetDictionaryPropertyName() =>
      nameof(LevelBagManifest.Levels);

    protected override string GetDictionaryDisplayLabel() =>
      "Levels  (Scene Name → BagConfig)";

    /// <summary>
    /// AutoFill key: asset file name without extension.
    /// After AutoFill the designer picks the right scene in the dropdown.
    /// </summary>
    protected override string GetKeyFromData(BagConfig data) => data.name;

    protected override IDictionary<string, AssetReferenceT<BagConfig>>
      GetDictionary(LevelBagManifest manifest) => manifest.Levels;

    /// <summary>Scene name dropdown for keys — uses InspectorUtils.GetAllScenes().</summary>
    protected override ICustomKeyDrawer CreateCustomKeyDrawer() =>
      new SceneDropdownKeyDrawer("BagConfig");

    protected override void DrawBeforeAutoFill()
    {
      EditorGUILayout.HelpBox(
        "Maps level scene names to BagConfig assets.\n\n" +
        "AutoFill adds all BagConfig assets found in the project (keyed by asset name).\n" +
        "Use the scene dropdown on each entry to assign the correct level name.\n\n" +
        "Level names must match the string passed to LoadLevelState (scene name).",
        MessageType.Info);
    }
  }
}
#endif
