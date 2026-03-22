// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

#if UNITY_EDITOR
using UnityEditor;

using UnityEditorInternal;

using UnityEngine;

namespace Code.Common.Extensions.CustomTypes.Types.Editor
{
  [CustomPropertyDrawer(typeof(Code.Common.CustomTypes.Domain.Collections.DictionaryData<,>))]
  public class DictionaryDataDrawer : PropertyDrawer
  {
    private const float COLUMN_SPACING = 10f;
    private const float VERTICAL_PADDING = 2f;

    // When a value is a collection wrapper, the key column gets this fraction of total width.
    private const float WRAPPER_KEY_RATIO = 0.28f;

    private readonly System.Collections.Generic.Dictionary<string, ReorderableList> _lists =
        new System.Collections.Generic.Dictionary<string, ReorderableList>();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
      DrawDictionary(position, property, label);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
      var list = GetOrCreateList(property, label);
      return list.GetHeight();
    }

    private void DrawDictionary(Rect position, SerializedProperty property, GUIContent label)
    {
      EnsureArraySynchronization(property);

      var list = GetOrCreateList(property, label);
      list.DoList(position);
    }

    private ReorderableList GetOrCreateList(SerializedProperty property, GUIContent label)
    {
      string key = property.propertyPath;
      if (!_lists.TryGetValue(key, out var list))
      {
        list = CreateReorderableList(property, label);
        _lists[key] = list;
      }
      return list;
    }

    private ReorderableList CreateReorderableList(SerializedProperty property, GUIContent label)
    {
      SerializedProperty keyArray = property.FindPropertyRelative("keyData");

      var list = new ReorderableList(
          property.serializedObject,
          keyArray,
          draggable: true,
          displayHeader: true,
          displayAddButton: true,
          displayRemoveButton: true
      );

      list.drawHeaderCallback = rect => DrawHeader(rect, label.text);
      list.elementHeightCallback = index => CalculateElementHeight(property, index);
      list.drawElementCallback = (rect, index, isActive, isFocused) =>
          DrawElement(rect, property, index);

      list.onAddCallback = _ => AddElement(property);
      list.onRemoveCallback = l => RemoveElement(property, l.index);

      list.headerHeight = EditorGUIUtility.singleLineHeight * 2 + VERTICAL_PADDING * 2;

      return list;
    }

    private void DrawHeader(Rect rect, string title)
    {
      Rect titleRect = new Rect(
          rect.x,
          rect.y,
          rect.width,
          EditorGUIUtility.singleLineHeight
      );

      EditorGUI.LabelField(titleRect, title, EditorStyles.boldLabel);

      float columnWidth = (rect.width - COLUMN_SPACING) / 2f;
      float labelY = rect.y + EditorGUIUtility.singleLineHeight + VERTICAL_PADDING;

      EditorGUI.LabelField(
          new Rect(rect.x, labelY, columnWidth, EditorGUIUtility.singleLineHeight),
          "Key",
          EditorStyles.miniLabel
      );

      EditorGUI.LabelField(
          new Rect(rect.x + columnWidth + COLUMN_SPACING, labelY, columnWidth, EditorGUIUtility.singleLineHeight),
          "Value",
          EditorStyles.miniLabel
      );
    }

    private void DrawElement(Rect rect, SerializedProperty property, int index)
    {
      SerializedProperty keyArray = property.FindPropertyRelative("keyData");
      SerializedProperty valueArray = property.FindPropertyRelative("valueData");

      if (!IsValidIndex(index, keyArray, valueArray))
        return;

      SerializedProperty keyElement = keyArray.GetArrayElementAtIndex(index);
      SerializedProperty valueElement = valueArray.GetArrayElementAtIndex(index);

      rect.y += EditorGUIUtility.standardVerticalSpacing;

      // If value is a wrapper class containing a single array/list,
      // draw the collection directly — no foldout, no wrapper label,
      // and give the value column more horizontal space.
      SerializedProperty collectionChild = FindCollectionChild(valueElement);
      bool isValueWrapper = collectionChild != null;

      GetColumnWidths(rect.width, isValueWrapper, out float keyWidth, out float valueWidth);

      float keyHeight = EditorGUI.GetPropertyHeight(keyElement, true);
      float drawHeight = isValueWrapper
          ? CalculateArrayHeight(collectionChild)
          : EditorGUI.GetPropertyHeight(valueElement, true);

      float rowHeight = Mathf.Max(keyHeight, drawHeight);

      Rect keyRect = new Rect(rect.x, rect.y, keyWidth, rowHeight);
      Rect valueRect = new Rect(rect.x + keyWidth + COLUMN_SPACING, rect.y, valueWidth, rowHeight);

      EditorGUI.PropertyField(keyRect, keyElement, GUIContent.none, true);

      if (isValueWrapper)
        DrawArrayWithoutElementLabels(valueRect, collectionChild);
      else
        EditorGUI.PropertyField(valueRect, valueElement, GUIContent.none, true);
    }

    private float CalculateElementHeight(SerializedProperty property, int index)
    {
      SerializedProperty keyArray = property.FindPropertyRelative("keyData");
      SerializedProperty valueArray = property.FindPropertyRelative("valueData");

      if (!IsValidIndex(index, keyArray, valueArray))
        return EditorGUIUtility.singleLineHeight;

      SerializedProperty keyElement = keyArray.GetArrayElementAtIndex(index);
      SerializedProperty valueElement = valueArray.GetArrayElementAtIndex(index);

      SerializedProperty collectionChild = FindCollectionChild(valueElement);

      float keyHeight = EditorGUI.GetPropertyHeight(keyElement, true);
      float valueHeight = collectionChild != null
          ? CalculateArrayHeight(collectionChild)
          : EditorGUI.GetPropertyHeight(valueElement, true);

      return Mathf.Max(keyHeight, valueHeight) +
             EditorGUIUtility.standardVerticalSpacing * 2;
    }

    private void AddElement(SerializedProperty property)
    {
      SerializedProperty keyArray = property.FindPropertyRelative("keyData");
      SerializedProperty valueArray = property.FindPropertyRelative("valueData");

      int index = keyArray.arraySize;

      keyArray.InsertArrayElementAtIndex(index);
      valueArray.InsertArrayElementAtIndex(index);

      // Unity copies the previous element's value on insert, which creates a duplicate key.
      // DictionaryData silently drops duplicates on deserialization, so we must set a unique value.
      SerializedProperty newKey = keyArray.GetArrayElementAtIndex(index);
      if (newKey.propertyType == SerializedPropertyType.String)
      {
        newKey.stringValue = GenerateUniqueStringKey(keyArray, index);
      }
      else if (newKey.propertyType == SerializedPropertyType.Enum)
      {
        newKey.enumValueIndex = GenerateUniqueEnumIndex(keyArray, index);
      }

      property.serializedObject.ApplyModifiedProperties();
    }

    private string GenerateUniqueStringKey(SerializedProperty keyArray, int newIndex)
    {
      const string baseName = "param_";
      int counter = newIndex;

      while (true)
      {
        string candidate = baseName + counter;
        bool taken = false;

        for (int i = 0; i < newIndex; i++)
        {
          if (keyArray.GetArrayElementAtIndex(i).stringValue == candidate)
          {
            taken = true;
            break;
          }
        }

        if (!taken)
          return candidate;

        counter++;
      }
    }

    private int GenerateUniqueEnumIndex(SerializedProperty keyArray, int newIndex)
    {
      // Collect all enum indices already in use (excluding the newly inserted slot).
      var usedIndices = new System.Collections.Generic.HashSet<int>();
      for (int i = 0; i < newIndex; i++)
        usedIndices.Add(keyArray.GetArrayElementAtIndex(i).enumValueIndex);

      // Find first unused value across all available enum names.
      int enumCount = keyArray.GetArrayElementAtIndex(newIndex).enumNames.Length;
      for (int i = 0; i < enumCount; i++)
      {
        if (!usedIndices.Contains(i))
          return i;
      }

      // All values exhausted — return 0, DictionaryData will drop it as a duplicate.
      return 0;
    }

    private void RemoveElement(SerializedProperty property, int index)
    {
      SerializedProperty keyArray = property.FindPropertyRelative("keyData");
      SerializedProperty valueArray = property.FindPropertyRelative("valueData");

      if (!IsValidIndex(index, keyArray, valueArray))
        return;

      keyArray.DeleteArrayElementAtIndex(index);
      valueArray.DeleteArrayElementAtIndex(index);

      property.serializedObject.ApplyModifiedProperties();
    }

    // Reusable blank label — avoids issues with the shared GUIContent.none
    // being mutated internally by Unity when drawing object reference fields.
    private static readonly GUIContent _emptyLabel = new GUIContent(string.Empty);

    /// <summary>
    /// Returns the total height needed to render an array via DrawArrayWithoutElementLabels.
    /// </summary>
    private float CalculateArrayHeight(SerializedProperty arrayProp)
    {
      float spacing = EditorGUIUtility.standardVerticalSpacing;

      // Size field.
      float total = EditorGUIUtility.singleLineHeight + spacing;

      for (int i = 0; i < arrayProp.arraySize; i++)
      {
        SerializedProperty element = arrayProp.GetArrayElementAtIndex(i);
        total += EditorGUI.GetPropertyHeight(element, _emptyLabel, true) + spacing;
      }

      return total;
    }

    /// <summary>
    /// Draws an array property without "Element 0 / Element 1 / ..." labels on each item.
    /// Renders the array size field first, then each element inline.
    /// </summary>
    private void DrawArrayWithoutElementLabels(Rect rect, SerializedProperty arrayProp)
    {
      float singleLine = EditorGUIUtility.singleLineHeight;
      float spacing = EditorGUIUtility.standardVerticalSpacing;

      // Size field on the first line.
      Rect sizeRect = new Rect(rect.x, rect.y, rect.width, singleLine);
      int newSize = EditorGUI.DelayedIntField(sizeRect, arrayProp.arraySize);
      if (newSize != arrayProp.arraySize)
      {
        arrayProp.arraySize = Mathf.Max(0, newSize);
        arrayProp.serializedObject.ApplyModifiedProperties();
      }

      float y = rect.y + singleLine + spacing;

      for (int i = 0; i < arrayProp.arraySize; i++)
      {
        SerializedProperty element = arrayProp.GetArrayElementAtIndex(i);
        float elementHeight = EditorGUI.GetPropertyHeight(element, _emptyLabel, true);
        Rect elementRect = new Rect(rect.x, y, rect.width, elementHeight);
        EditorGUI.PropertyField(elementRect, element, _emptyLabel, true);
        y += elementHeight + spacing;
      }
    }

    private void EnsureArraySynchronization(SerializedProperty property)
    {
      SerializedProperty keyArray = property.FindPropertyRelative("keyData");
      SerializedProperty valueArray = property.FindPropertyRelative("valueData");

      if (keyArray == null || valueArray == null)
        return;

      if (keyArray.arraySize != valueArray.arraySize)
      {
        int min = Mathf.Min(keyArray.arraySize, valueArray.arraySize);
        keyArray.arraySize = min;
        valueArray.arraySize = min;
        property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
      }
    }

    /// <summary>
    /// If property is a Generic serialized class that contains exactly one
    /// visible array or list field, returns that field. Otherwise returns null.
    /// This lets the drawer detect wrapper types like AudioClipGroup { AudioClip[] clips }.
    /// </summary>
    private SerializedProperty FindCollectionChild(SerializedProperty property)
    {
      if (property == null || property.propertyType != SerializedPropertyType.Generic)
        return null;

      SerializedProperty iterator = property.Copy();
      SerializedProperty end = iterator.GetEndProperty();

      if (!iterator.NextVisible(true))
        return null;

      SerializedProperty found = null;
      int visibleCount = 0;

      while (!SerializedProperty.EqualContents(iterator, end))
      {
        visibleCount++;

        // More than one visible child — not a simple wrapper, bail out.
        if (visibleCount > 1)
          return null;

        if (iterator.isArray)
          found = iterator.Copy();

        if (!iterator.NextVisible(false))
          break;
      }

      return found;
    }

    private static void GetColumnWidths(float totalWidth, bool isValueWrapper,
        out float keyWidth, out float valueWidth)
    {
      if (isValueWrapper)
      {
        keyWidth = totalWidth * WRAPPER_KEY_RATIO - COLUMN_SPACING / 2f;
        valueWidth = totalWidth * (1f - WRAPPER_KEY_RATIO) - COLUMN_SPACING / 2f;
      }
      else
      {
        keyWidth = (totalWidth - COLUMN_SPACING) / 2f;
        valueWidth = keyWidth;
      }
    }

    private bool IsValidIndex(int index, SerializedProperty keyArray, SerializedProperty valueArray)
    {
      return index >= 0 &&
             index < keyArray.arraySize &&
             index < valueArray.arraySize;
    }
  }
}
#endif
