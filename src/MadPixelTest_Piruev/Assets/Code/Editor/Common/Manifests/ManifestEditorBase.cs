// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

#if UNITY_EDITOR
using System.Collections.Generic;

using Code.Common.CustomTypes.Domain.Collections;
using Code.Editor.Common.Manifests.Interfaces;

using UnityEditor;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Code.Editor.Common.Manifests
{
  /// <summary>
  /// Base class for manifest editors that provides AutoFill functionality.
  /// Designed to work with DictionaryData + DictionaryDataDrawer.
  /// </summary>
  public abstract class ManifestEditorBase<TManifest, TData, TKey> : ManualSaveEditor
      where TManifest : ScriptableObject
      where TData : ScriptableObject
  {
    private SerializedProperty _dictionaryProperty;
    private ICustomKeyDrawer _customKeyDrawer;
    private bool _useCustomDrawer;

    #region Lifecycle

    private void OnEnable()
    {
      _dictionaryProperty = serializedObject.FindProperty(GetDictionaryPropertyName());

      _customKeyDrawer = CreateCustomKeyDrawer();
      _useCustomDrawer = _customKeyDrawer != null;

      OnEnableCustom();
    }

    protected override void OnDisable()
    {
      _customKeyDrawer?.ClearCache();
      OnDisableCustom();
      base.OnDisable();
    }

    #endregion

    #region Abstract API

    protected abstract string GetDictionaryPropertyName();
    protected abstract string GetDictionaryDisplayLabel();
    protected abstract TKey GetKeyFromData(TData data);

    protected abstract IDictionary<TKey, AssetReferenceT<TData>>
        GetDictionary(TManifest manifest);

    #endregion

    #region Optional Overrides

    protected virtual void OnEnableCustom() { }
    protected virtual void OnDisableCustom() { }
    protected virtual void DrawBeforeAutoFill() { }
    protected virtual void DrawAfterDictionary() { }

    protected virtual ICustomKeyDrawer CreateCustomKeyDrawer() => null;

    #endregion

    #region Inspector Drawing

    protected override void DrawInspector()
    {
      EditorGUILayout.Space(10);

      DrawBeforeAutoFill();
      DrawAutoFillButton();

      EditorGUILayout.Space(10);

      DrawDictionary();
      DrawAfterDictionary();
    }

    private void DrawAutoFillButton()
    {
      EditorGUILayout.BeginHorizontal();
      GUILayout.FlexibleSpace();

      if (GUILayout.Button("AutoFill from Assets", GUILayout.Height(30), GUILayout.Width(200)))
      {
        PerformAutoFill();
      }

      GUILayout.FlexibleSpace();
      EditorGUILayout.EndHorizontal();
    }

    private void DrawDictionary()
    {
      if (_dictionaryProperty == null)
        return;

      if (_useCustomDrawer && _customKeyDrawer != null)
      {
        _customKeyDrawer.DrawDictionaryWithCustomKeys(
            _dictionaryProperty,
            new GUIContent(GetDictionaryDisplayLabel())
        );
      }
      else
      {
        // IMPORTANT:
        // We rely on DictionaryDataDrawer now.
        EditorGUILayout.PropertyField(
            _dictionaryProperty,
            new GUIContent(GetDictionaryDisplayLabel()),
            includeChildren: true
        );
      }
    }

    #endregion

    #region AutoFill

    private void PerformAutoFill()
    {
      string[] guids = AssetDatabase.FindAssets($"t:{typeof(TData).Name}");

      if (guids.Length == 0)
      {
        ShowNoAssetsFoundDialog();
        return;
      }

      Undo.RecordObject(target, "AutoFill Manifest");

      var result = ProcessAssets(guids);

      var manifest = (TManifest)target;
      var dictionary = GetDictionary(manifest);

      // Synchronize managed dictionary → serialized lists
      if (dictionary is IForceSerialization forceSerialization)
      {
        forceSerialization.ForceSerialization();
      }

      EditorUtility.SetDirty(target);

      // IMPORTANT:
      // We changed managed data directly,
      // so we just force SerializedObject to refresh.
      serializedObject.Update();

      Repaint();

      ShowAutoFillSummary(result);
    }

    private AutoFillResult ProcessAssets(string[] guids)
    {
      var result = new AutoFillResult();
      var manifest = (TManifest)target;
      var dictionary = GetDictionary(manifest);

      foreach (string guid in guids)
      {
        string assetPath = AssetDatabase.GUIDToAssetPath(guid);
        var data = AssetDatabase.LoadAssetAtPath<TData>(assetPath);

        if (data == null)
          continue;

        TKey key = GetKeyFromData(data);

        if (dictionary.ContainsKey(key))
        {
          UpdateExistingEntry(dictionary, key, assetPath, guid, ref result);
        }
        else
        {
          AddNewEntry(dictionary, key, guid, ref result);
        }
      }

      return result;
    }

    private void UpdateExistingEntry(
        IDictionary<TKey, AssetReferenceT<TData>> dictionary,
        TKey key,
        string assetPath,
        string guid,
        ref AutoFillResult result)
    {
      var existingReference = dictionary[key];
      string existingPath = AssetDatabase.GUIDToAssetPath(existingReference.AssetGUID);

      if (existingPath != assetPath)
      {
        dictionary[key] = new AssetReferenceT<TData>(guid);
        result.UpdatedCount++;
      }
      else
      {
        result.SkippedCount++;
      }
    }

    private void AddNewEntry(
        IDictionary<TKey, AssetReferenceT<TData>> dictionary,
        TKey key,
        string guid,
        ref AutoFillResult result)
    {
      dictionary.Add(key, new AssetReferenceT<TData>(guid));
      result.AddedCount++;
    }

    #endregion

    #region Dialogs

    private void ShowNoAssetsFoundDialog()
    {
      EditorUtility.DisplayDialog(
          "No Assets Found",
          $"No {typeof(TData).Name} assets were found in the project.",
          "OK"
      );
    }

    private void ShowAutoFillSummary(AutoFillResult result)
    {
      var manifest = (TManifest)target;
      var dictionary = GetDictionary(manifest);

      string message =
          $"AutoFill completed:\n\n" +
          $"• Added: {result.AddedCount}\n" +
          $"• Updated: {result.UpdatedCount}\n" +
          $"• Skipped: {result.SkippedCount}\n\n" +
          $"Total entries: {dictionary.Count}";

      EditorUtility.DisplayDialog("AutoFill Complete", message, "OK");
    }

    #endregion

    private struct AutoFillResult
    {
      public int AddedCount;
      public int UpdatedCount;
      public int SkippedCount;
    }
  }
}
#endif
