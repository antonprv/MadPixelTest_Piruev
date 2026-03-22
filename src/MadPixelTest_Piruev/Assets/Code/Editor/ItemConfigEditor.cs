using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BagFight.Data;

namespace BagFight.Editor
{
  /// <summary>
  /// Кастомный инспектор для ItemConfig.
  /// Рисует интерактивную сетку формы предмета: кликаешь по клетке — добавляешь/убираешь.
  /// Размер превью ограничен bounding box'ом предмета, но не менее 1×1.
  /// </summary>
  [CustomEditor(typeof(ItemConfig))]
  public class ItemConfigEditor : UnityEditor.Editor
  {
    private const int   CellSize    = 28;
    private const int   CellPad     = 2;
    private const int   MaxPreview  = 8; // максимум 8×8 превью

    private SerializedProperty _itemId;
    private SerializedProperty _level;
    private SerializedProperty _icon;
    private SerializedProperty _itemColor;
    private SerializedProperty _shape;
    private SerializedProperty _mergeResult;

    private void OnEnable()
    {
      _itemId      = serializedObject.FindProperty("<ItemId>k__BackingField");
      _level       = serializedObject.FindProperty("<Level>k__BackingField");
      _icon        = serializedObject.FindProperty("<Icon>k__BackingField");
      _itemColor   = serializedObject.FindProperty("<ItemColor>k__BackingField");
      _shape       = serializedObject.FindProperty("<Shape>k__BackingField");
      _mergeResult = serializedObject.FindProperty("<MergeResult>k__BackingField");
    }

    public override void OnInspectorGUI()
    {
      serializedObject.Update();

      // ── Базовые поля ──────────────────────────────────────────────────────
      EditorGUILayout.PropertyField(_itemId,      new GUIContent("Item Id"));
      EditorGUILayout.PropertyField(_level,       new GUIContent("Level"));
      // Icon — AssetReferenceSprite, стандартный PropertyField рисует Addressable-picker
      EditorGUILayout.PropertyField(_icon,        new GUIContent("Icon (Addressable)"));
      EditorGUILayout.PropertyField(_itemColor,   new GUIContent("Item Color"));
      EditorGUILayout.PropertyField(_mergeResult, new GUIContent("Merge Result"));

      EditorGUILayout.Space(8);

      // ── Shape ─────────────────────────────────────────────────────────────
      EditorGUILayout.LabelField("Shape", EditorStyles.boldLabel);
      EditorGUILayout.HelpBox(
        "Кликни по клетке чтобы добавить / убрать её из формы.\n" +
        "Первая клетка (0,0) — верхний левый угол bounding box.",
        MessageType.Info);

      DrawShapeGrid();

      EditorGUILayout.Space(4);
      EditorGUILayout.PropertyField(_shape, new GUIContent("Raw Shape (Vector2Int list)"), true);

      serializedObject.ApplyModifiedProperties();
    }

    private void DrawShapeGrid()
    {
      var cfg    = (ItemConfig)target;
      var shape  = cfg.Shape ?? new List<Vector2Int>();
      var bounds = cfg.GetBoundsSize();

      // Рисуем сетку не меньше 3×3 для удобства редактирования
      int cols = Mathf.Clamp(bounds.x + 1, 3, MaxPreview);
      int rows = Mathf.Clamp(bounds.y + 1, 3, MaxPreview);

      var step      = CellSize + CellPad;
      var totalW    = cols * step - CellPad;
      var totalH    = rows * step - CellPad;
      var startRect = GUILayoutUtility.GetRect(totalW, totalH);

      // Цвет фона из ItemColor
      var filledColor = cfg.ItemColor == default ? new Color(0.35f, 0.55f, 1f, 0.9f) : cfg.ItemColor;
      var emptyColor  = new Color(0.18f, 0.18f, 0.18f, 0.7f);
      var borderColor = new Color(0.08f, 0.08f, 0.08f, 1f);

      for (int y = 0; y < rows; y++)
      for (int x = 0; x < cols; x++)
      {
        var coord = new Vector2Int(x, y);
        var isFilled = shape.Contains(coord);

        var rect = new Rect(
          startRect.x + x * step,
          startRect.y + y * step,
          CellSize, CellSize
        );

        // Граница
        EditorGUI.DrawRect(rect, borderColor);
        // Заливка
        var innerRect = new Rect(rect.x + 1, rect.y + 1, rect.width - 2, rect.height - 2);
        EditorGUI.DrawRect(innerRect, isFilled ? filledColor : emptyColor);

        // Координата (мелкий текст)
        var labelStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
        {
          fontSize  = 8,
          alignment = TextAnchor.LowerRight,
          normal    = { textColor = new Color(1, 1, 1, 0.35f) }
        };
        EditorGUI.LabelField(innerRect, $"{x},{y}", labelStyle);

        // Клик — тоглим клетку
        if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
        {
          Undo.RecordObject(target, "Toggle shape cell");

          // Работаем напрямую с serializedObject
          bool found = false;
          for (int i = 0; i < _shape.arraySize; i++)
          {
            var el = _shape.GetArrayElementAtIndex(i);
            var v  = el.vector2IntValue;
            if (v == coord)
            {
              _shape.DeleteArrayElementAtIndex(i);
              found = true;
              break;
            }
          }

          if (!found)
          {
            _shape.arraySize++;
            _shape.GetArrayElementAtIndex(_shape.arraySize - 1).vector2IntValue = coord;
          }

          serializedObject.ApplyModifiedProperties();
          Event.current.Use();
        }
      }

      // Сетка нуждается в постоянном repaint при hover
      if (Event.current.type == EventType.MouseMove)
        Repaint();
    }
  }
}
