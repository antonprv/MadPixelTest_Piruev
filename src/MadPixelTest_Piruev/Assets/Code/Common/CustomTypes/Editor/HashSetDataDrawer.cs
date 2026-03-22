// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

#if UNITY_EDITOR
using Code.Common.CustomTypes.Domain.Collections;

using UnityEditor;

using UnityEngine;

namespace Code.Common.Extensions.CustomTypes.Types.Editor
{
  [CustomPropertyDrawer(typeof(HashSetData<>))]
  public class HashSetDataDrawer : PropertyDrawer
  {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
      EditorGUI.BeginProperty(position, label, property);

      var dataProperty = property.FindPropertyRelative("data");
      if (dataProperty != null)
      {
        EditorGUI.PropertyField(position, dataProperty, label, true);
      }

      EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
      var dataProperty = property.FindPropertyRelative("data");
      if (dataProperty != null)
      {
        return EditorGUI.GetPropertyHeight(dataProperty, label, true);
      }
      return EditorGUIUtility.singleLineHeight;
    }
  }
}
#endif
