// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System.IO;
using System.Linq;

using UnityEditor;

using UnityEngine;

namespace Code.Editor.Common
{
  public class InspectorUtils : UnityEditor.Editor
  {
    public static Color fleaBellyColor = new Color(78f, 22f, 9f);

    public static void DrawFoldout(
      SerializedObject serializedObject,
      string title,
      ref bool state,
      string[] fieldNames)
    {
      state = EditorGUILayout.BeginFoldoutHeaderGroup(state, title);

      if (state)
      {
        foreach (string field in fieldNames)
        {
          SerializedProperty property = serializedObject.FindProperty(field);
          EditorGUILayout.PropertyField(property, true);
        }
      }

      EditorGUILayout.EndFoldoutHeaderGroup();
    }

    public static string[] GetAllScenes()
    {
      return Directory
        .GetFiles("Assets", "*.unity", SearchOption.AllDirectories)
        .Select(Path.GetFileNameWithoutExtension)
        .OrderBy(n => n)
        .ToArray();
    }
  }
}
