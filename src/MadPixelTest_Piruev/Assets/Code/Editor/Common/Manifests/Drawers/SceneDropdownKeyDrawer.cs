// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

#if UNITY_EDITOR
using System.Collections.Generic;

using Code.Editor.Common.Manifests.Interfaces;

using UnityEditor;

using UnityEngine;

namespace Code.Editor.Common.Manifests.Drawers
{
  /// <summary>
  /// Custom key drawer that renders string keys as scene name dropdowns.
  /// Uses <see cref="InspectorUtils.GetAllScenes"/> to populate the dropdown.
  /// </summary>
  public class SceneDropdownKeyDrawer : ICustomKeyDrawer
  {
    #region Constants

    private const string DefaultValueName = "Level Data";

    private static readonly Color AddButtonColor = new(0.7f, 1f, 0.7f);
    private static readonly Color RemoveButtonColor = new(1f, 0.5f, 0.5f);

    #endregion

    #region Fields

    private string[] _availableScenes;
    private Dictionary<string, int> _sceneIndexCache = new();
    private readonly string _valueName;

    #endregion

    #region Constructor

    public SceneDropdownKeyDrawer(string valueName = DefaultValueName)
    {
      _valueName = valueName;
      RefreshSceneList();
    }

    #endregion

    #region ICustomKeyDrawer

    public void ClearCache() =>
      _sceneIndexCache.Clear();

    public void DrawDictionaryWithCustomKeys(SerializedProperty property, GUIContent label)
    {
      EnsureArraySynchronization(property);

      var keyArray = property.FindPropertyRelative("keyData");
      var valueArray = property.FindPropertyRelative("valueData");

      DrawHeader(label);
      DrawRefreshBar();
      DrawEntries(property, keyArray, valueArray);
      DrawAddButton(property, keyArray, valueArray);
    }

    #endregion

    #region Drawing

    private void DrawHeader(GUIContent label)
    {
      EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
      EditorGUILayout.Space(5);
    }

    private void DrawRefreshBar()
    {
      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.LabelField($"Available Scenes: {_availableScenes.Length}", EditorStyles.miniLabel);
      GUILayout.FlexibleSpace();

      if (GUILayout.Button("Refresh Scene List", GUILayout.Width(130), GUILayout.Height(22)))
        RefreshSceneList();

      EditorGUILayout.EndHorizontal();
      EditorGUILayout.Space(5);
    }

    private void DrawEntries(SerializedProperty property, SerializedProperty keyArray, SerializedProperty valueArray)
    {
      for (int i = keyArray.arraySize - 1; i >= 0; i--)
      {
        bool removed = DrawEntry(keyArray, valueArray, i);

        if (removed)
          property.serializedObject.ApplyModifiedProperties();
      }

      EditorGUILayout.Space(5);
    }

    private bool DrawEntry(SerializedProperty keyArray, SerializedProperty valueArray, int index)
    {
      EditorGUILayout.BeginVertical(EditorStyles.helpBox);
      EditorGUILayout.BeginHorizontal();

      bool removed = false;

      DrawEntryFields(keyArray, valueArray, index);
      removed = DrawRemoveButton(keyArray, valueArray, index);

      EditorGUILayout.EndHorizontal();
      EditorGUILayout.EndVertical();
      EditorGUILayout.Space(3);

      return removed;
    }

    private void DrawEntryFields(SerializedProperty keyArray, SerializedProperty valueArray, int index)
    {
      var keyProperty = keyArray.GetArrayElementAtIndex(index);
      string currentKey = keyProperty.stringValue;
      int currentIndex = GetSceneIndex(currentKey);

      EditorGUILayout.BeginVertical();

      int newIndex = EditorGUILayout.Popup("Scene", currentIndex, _availableScenes);

      if (newIndex != currentIndex && newIndex >= 0 && newIndex < _availableScenes.Length)
        keyProperty.stringValue = _availableScenes[newIndex];

      var valueProperty = valueArray.GetArrayElementAtIndex(index);
      EditorGUILayout.PropertyField(valueProperty, new GUIContent(_valueName), includeChildren: true);

      EditorGUILayout.EndVertical();
    }

    private bool DrawRemoveButton(SerializedProperty keyArray, SerializedProperty valueArray, int index)
    {
      string sceneName = keyArray.GetArrayElementAtIndex(index).stringValue;
      bool removed = false;

      GUI.backgroundColor = RemoveButtonColor;

      if (GUILayout.Button("×", GUILayout.Width(30), GUILayout.Height(40)))
      {
        bool confirmed = EditorUtility.DisplayDialog(
          title: "Remove Entry",
          message: $"Remove level entry for scene '{sceneName}'?",
          ok: "Remove",
          cancel: "Cancel");

        if (confirmed)
        {
          keyArray.DeleteArrayElementAtIndex(index);
          valueArray.DeleteArrayElementAtIndex(index);
          removed = true;
        }
      }

      GUI.backgroundColor = Color.white;
      return removed;
    }

    private void DrawAddButton(SerializedProperty property, SerializedProperty keyArray, SerializedProperty valueArray)
    {
      GUI.backgroundColor = AddButtonColor;

      if (GUILayout.Button("Add Entry", GUILayout.Height(28)))
      {
        keyArray.InsertArrayElementAtIndex(keyArray.arraySize);
        valueArray.InsertArrayElementAtIndex(valueArray.arraySize);

        if (_availableScenes.Length > 0)
          keyArray.GetArrayElementAtIndex(keyArray.arraySize - 1).stringValue = GetNextUnusedScene(keyArray);

        property.serializedObject.ApplyModifiedProperties();
      }

      GUI.backgroundColor = Color.white;
    }

    #endregion

    #region Scene List

    private void RefreshSceneList()
    {
      _availableScenes = InspectorUtils.GetAllScenes();
      _sceneIndexCache.Clear();

      for (int i = 0; i < _availableScenes.Length; i++)
        _sceneIndexCache[_availableScenes[i]] = i;
    }

    private string GetNextUnusedScene(SerializedProperty keyArray)
    {
      var usedKeys = new HashSet<string>();

      // Exclude the newly inserted last element — it has no meaningful value yet
      for (int i = 0; i < keyArray.arraySize - 1; i++)
        usedKeys.Add(keyArray.GetArrayElementAtIndex(i).stringValue);

      foreach (string scene in _availableScenes)
        if (!usedKeys.Contains(scene))
          return scene;

      return _availableScenes[0];
    }

    private int GetSceneIndex(string sceneName)
    {
      if (string.IsNullOrEmpty(sceneName))
        return 0;

      return _sceneIndexCache.TryGetValue(sceneName, out int index) ? index : 0;
    }

    #endregion

    #region Validation

    private static void EnsureArraySynchronization(SerializedProperty property)
    {
      var keyArray = property.FindPropertyRelative("keyData");
      var valueArray = property.FindPropertyRelative("valueData");

      if (keyArray.arraySize != valueArray.arraySize)
      {
        int syncedSize = Mathf.Min(keyArray.arraySize, valueArray.arraySize);
        keyArray.arraySize = syncedSize;
        valueArray.arraySize = syncedSize;
      }
    }

    #endregion
  }
}
#endif
