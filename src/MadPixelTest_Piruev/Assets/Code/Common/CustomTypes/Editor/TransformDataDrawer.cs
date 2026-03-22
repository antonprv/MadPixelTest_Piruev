// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

#if UNITY_EDITOR
using Code.Common.Domain.DataTypes;

using UnityEditor;

using UnityEngine;

namespace Code.Common.Extensions.CustomTypes.Types.Editor
{
  [CustomPropertyDrawer(typeof(TransformData))]
  public class TransformDataDrawer : PropertyDrawer
  {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
      EditorGUI.BeginProperty(position, label, property);

      property.isExpanded = EditorGUI.Foldout(
        new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
        property.isExpanded,
        label,
        true
      );

      if (property.isExpanded)
      {
        EditorGUI.indentLevel++;

        var positionProperty = property.FindPropertyRelative("Position");
        var rotationProperty = property.FindPropertyRelative("Rotation");
        var scaleProperty = property.FindPropertyRelative("Scale");

        float yOffset = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        // Draw Position
        if (positionProperty != null)
        {
          EditorGUI.PropertyField(
            new Rect(position.x, yOffset, position.width, EditorGUIUtility.singleLineHeight),
            positionProperty,
            new GUIContent("Position")
          );
          yOffset += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        // Draw Rotation
        if (rotationProperty != null)
        {
          EditorGUI.PropertyField(
            new Rect(position.x, yOffset, position.width, EditorGUIUtility.singleLineHeight),
            rotationProperty,
            new GUIContent("Rotation")
          );
          yOffset += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        // Draw Scale
        if (scaleProperty != null)
        {
          EditorGUI.PropertyField(
            new Rect(position.x, yOffset, position.width, EditorGUIUtility.singleLineHeight),
            scaleProperty,
            new GUIContent("Scale")
          );
        }

        EditorGUI.indentLevel--;
      }

      EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
      if (!property.isExpanded)
      {
        return EditorGUIUtility.singleLineHeight;
      }

      // Foldout + 3 Vector3 fields
      float height = EditorGUIUtility.singleLineHeight; // Foldout
      height += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 3; // Position, Rotation, Scale

      return height;
    }
  }
}
#endif
