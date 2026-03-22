// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

#if UNITY_EDITOR
using System;
using System.Linq;

using UnityEditor;

using UnityEngine;

[CustomPropertyDrawer(typeof(FilteredEnumAttribute))]
public class FilteredEnumDrawer : PropertyDrawer
{
  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
  {
    if (property.propertyType != SerializedPropertyType.Enum)
    {
      EditorGUI.PropertyField(position, property, label);
      return;
    }

    var attributeInstance = (FilteredEnumAttribute)attribute;

    Type enumType = fieldInfo.FieldType;
    var excluded = attributeInstance.ExcludedValues;

    var availableValues = Enum.GetValues(enumType)
        .Cast<Enum>()
        .Where(value => !excluded.Contains(value))
        .ToArray();

    if (availableValues.Length == 0)
    {
      EditorGUI.LabelField(position, label.text, "No available values");
      return;
    }

    Enum currentValue = (Enum)Enum.ToObject(enumType, property.enumValueIndex);

    int selectedIndex = Array.IndexOf(availableValues, currentValue);

    if (selectedIndex < 0)
    {
      selectedIndex = 0;
      property.enumValueIndex = Convert.ToInt32(availableValues[0]);
    }

    string[] displayNames = availableValues
        .Select(v => v.ToString())
        .ToArray();

    EditorGUI.BeginProperty(position, label, property);
    EditorGUI.BeginChangeCheck();

    int newIndex = EditorGUI.Popup(position, label.text, selectedIndex, displayNames);

    if (EditorGUI.EndChangeCheck())
    {
      property.enumValueIndex = Convert.ToInt32(availableValues[newIndex]);
    }

    EditorGUI.EndProperty();
  }
}

#endif
