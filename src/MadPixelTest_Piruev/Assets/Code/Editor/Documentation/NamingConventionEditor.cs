// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using UnityEditor;

using UnityEngine;

namespace Code.Editor.Documentation
{
  [CustomEditor(typeof(NamingConvention))]
  public class NamingConventionEditor : UnityEditor.Editor
  {
    public override void OnInspectorGUI()
    {
      NamingConvention convention = (NamingConvention)target;

      EditorGUILayout.Space(10);

      GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
      titleStyle.fontSize = 14;
      titleStyle.alignment = TextAnchor.MiddleCenter;

      EditorGUILayout.LabelField("PROJECT NAMING CONVENTION", titleStyle);

      EditorGUILayout.Space(10);

      GUIStyle textStyle = new GUIStyle(EditorStyles.label);
      textStyle.wordWrap = true;
      textStyle.richText = true;

      EditorGUILayout.LabelField(convention.conventionText, textStyle);
    }
  }
}
