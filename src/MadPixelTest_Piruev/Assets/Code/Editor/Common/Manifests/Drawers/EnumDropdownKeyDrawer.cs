// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

#if UNITY_EDITOR
using Code.Editor.Common.Manifests.Interfaces;

using UnityEditor;

using UnityEngine;

namespace Code.Editor.Common.Manifests.Drawers
{
  /// <summary>
  /// Custom key drawer that renders enum keys as dropdowns.
  /// Works with any enum type.
  /// </summary>
  /// <typeparam name="TEnum">The enum type for keys</typeparam>
  public class EnumDropdownKeyDrawer<TEnum> : ICustomKeyDrawer where TEnum : System.Enum
  {
    private TEnum[] _allEnumValues;
    private string[] _enumNames;
    private System.Collections.Generic.Dictionary<int, int> _valueToIndexCache
        = new System.Collections.Generic.Dictionary<int, int>();

    public EnumDropdownKeyDrawer()
    {
      RefreshEnumList();
    }

    public void ClearCache()
    {
      _valueToIndexCache.Clear();
    }

    public void DrawDictionaryWithCustomKeys(SerializedProperty property, GUIContent label)
    {
      EnsureArraySynchronization(property);

      var keyArray = property.FindPropertyRelative("keyData");
      var valueArray = property.FindPropertyRelative("valueData");

      // Header
      EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
      EditorGUILayout.Space(5);

      // Info line
      EditorGUILayout.LabelField($"Available Options: {_enumNames.Length}", EditorStyles.miniLabel);
      EditorGUILayout.Space(5);

      // Draw entries
      for (int i = keyArray.arraySize - 1; i >= 0; i--)
      {
        if (DrawEntry(keyArray, valueArray, i))
        {
          // Entry was deleted, apply changes
          property.serializedObject.ApplyModifiedProperties();
        }
      }

      EditorGUILayout.Space(5);

      // Add button
      GUI.backgroundColor = new Color(0.7f, 1f, 0.7f);
      if (GUILayout.Button("Add Entry", GUILayout.Height(28)))
      {
        keyArray.InsertArrayElementAtIndex(keyArray.arraySize);
        valueArray.InsertArrayElementAtIndex(valueArray.arraySize);

        // Set default value to first enum value
        if (_allEnumValues.Length > 0)
        {
          var newKeyProp = keyArray.GetArrayElementAtIndex(keyArray.arraySize - 1);
          newKeyProp.enumValueIndex = 0;
        }

        property.serializedObject.ApplyModifiedProperties();
      }
      GUI.backgroundColor = Color.white;
    }

    private bool DrawEntry(SerializedProperty keyArray, SerializedProperty valueArray, int index)
    {
      EditorGUILayout.BeginVertical(EditorStyles.helpBox);

      EditorGUILayout.BeginHorizontal();

      // Enum dropdown for key
      var keyProperty = keyArray.GetArrayElementAtIndex(index);

      EditorGUILayout.BeginVertical();

      // Draw enum popup
      int currentEnumIndex = keyProperty.enumValueIndex;
      string currentEnumName = currentEnumIndex >= 0 && currentEnumIndex < _enumNames.Length
          ? _enumNames[currentEnumIndex]
          : "Unknown";

      int newEnumIndex = EditorGUILayout.Popup("Type", currentEnumIndex, _enumNames);
      if (newEnumIndex != currentEnumIndex && newEnumIndex >= 0 && newEnumIndex < _allEnumValues.Length)
      {
        keyProperty.enumValueIndex = newEnumIndex;
      }

      // Value property
      var valueProperty = valueArray.GetArrayElementAtIndex(index);
      EditorGUILayout.PropertyField(valueProperty, new GUIContent("Data"), true);
      EditorGUILayout.EndVertical();

      // Remove button
      GUI.backgroundColor = new Color(1f, 0.5f, 0.5f);
      bool removed = false;
      if (GUILayout.Button("×", GUILayout.Width(30), GUILayout.Height(40)))
      {
        if (EditorUtility.DisplayDialog(
            "Remove Entry",
            $"Remove entry for '{currentEnumName}'?",
            "Remove",
            "Cancel"))
        {
          keyArray.DeleteArrayElementAtIndex(index);
          valueArray.DeleteArrayElementAtIndex(index);
          removed = true;
        }
      }
      GUI.backgroundColor = Color.white;

      EditorGUILayout.EndHorizontal();
      EditorGUILayout.EndVertical();
      EditorGUILayout.Space(3);

      return removed;
    }

    private void RefreshEnumList()
    {
      _allEnumValues = (TEnum[])System.Enum.GetValues(typeof(TEnum));
      _enumNames = System.Enum.GetNames(typeof(TEnum));
      _valueToIndexCache.Clear();

      for (int i = 0; i < _allEnumValues.Length; i++)
      {
        int enumValue = System.Convert.ToInt32(_allEnumValues[i]);
        _valueToIndexCache[enumValue] = i;
      }
    }

    private void EnsureArraySynchronization(SerializedProperty property)
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
  }
}
#endif
