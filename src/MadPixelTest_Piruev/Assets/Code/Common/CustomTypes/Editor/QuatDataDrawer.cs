// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

#if UNITY_EDITOR
using Code.Common.Domain.DataTypes;

using UnityEditor;

using UnityEngine;

namespace Code.Common.Extensions.CustomTypes.Types.Editor
{
  [CustomPropertyDrawer(typeof(QuatData))]
  public class QuatDataDrawer : PropertyDrawer
  {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
      EditorGUI.BeginProperty(position, label, property);

      var xProperty = property.FindPropertyRelative("X");
      var yProperty = property.FindPropertyRelative("Y");
      var zProperty = property.FindPropertyRelative("Z");
      var wProperty = property.FindPropertyRelative("W");

      if (xProperty != null && yProperty != null && zProperty != null && wProperty != null)
      {
        Quaternion quaternion = new Quaternion(
          xProperty.floatValue,
          yProperty.floatValue,
          zProperty.floatValue,
          wProperty.floatValue
        );

        Vector3 eulerAngles = quaternion.eulerAngles;

        EditorGUI.BeginChangeCheck();
        eulerAngles = EditorGUI.Vector3Field(position, label, eulerAngles);

        if (EditorGUI.EndChangeCheck())
        {
          quaternion = Quaternion.Euler(eulerAngles);
          xProperty.floatValue = quaternion.x;
          yProperty.floatValue = quaternion.y;
          zProperty.floatValue = quaternion.z;
          wProperty.floatValue = quaternion.w;
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
