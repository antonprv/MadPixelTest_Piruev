// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

#if UNITY_EDITOR
using Code.Common.Domain.DataTypes;

using UnityEditor;

using UnityEngine;

namespace Code.Common.Extensions.CustomTypes.Types.Editor
{
  [CustomPropertyDrawer(typeof(Vector3Data))]
  public class Vector3DataDrawer : PropertyDrawer
  {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
      EditorGUI.BeginProperty(position, label, property);

      var xProperty = property.FindPropertyRelative("X");
      var yProperty = property.FindPropertyRelative("Y");
      var zProperty = property.FindPropertyRelative("Z");

      if (xProperty != null && yProperty != null && zProperty != null)
      {
        Vector3 vector = new Vector3(
          xProperty.floatValue,
          yProperty.floatValue,
          zProperty.floatValue
        );

        EditorGUI.BeginChangeCheck();
        vector = EditorGUI.Vector3Field(position, label, vector);

        if (EditorGUI.EndChangeCheck())
        {
          xProperty.floatValue = vector.x;
          yProperty.floatValue = vector.y;
          zProperty.floatValue = vector.z;
        }
      }

      EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
      return EditorGUIUtility.singleLineHeight;
    }
  }
}
#endif
