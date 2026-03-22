// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

using Code.Data.StaticData.Configs;

using Code.Editor.Common;

using UnityEditor;

using UnityEngine;

namespace Code.Editor
{
  /// <summary>
  /// Custom inspector for ItemConfig.
  /// Draws an interactive grid for item shape: click on a cell to add/remove it.
  /// Preview size is limited by item's bounding box, but at least 1×1.
  /// Extends ManualSaveEditor — changes are saved explicitly via the Save button.
  /// </summary>
  [CustomEditor(typeof(ItemConfig))]
  public class ItemConfigEditor : ManualSaveEditor
  {
    private const int CellSize = 28;
    private const int CellPad = 2;
    private const int MaxPreview = 8; // maximum 8×8 preview

    private SerializedProperty _itemId;
    private SerializedProperty _level;
    private SerializedProperty _icon;
    private SerializedProperty _itemColor;
    private SerializedProperty _shape;
    private SerializedProperty _mergeResult;

    private void OnEnable()
    {
      _itemId = serializedObject.FindProperty("<ItemId>k__BackingField");
      _level = serializedObject.FindProperty("<Level>k__BackingField");
      _icon = serializedObject.FindProperty("<Icon>k__BackingField");
      _itemColor = serializedObject.FindProperty("<ItemColor>k__BackingField");
      _shape = serializedObject.FindProperty("<Shape>k__BackingField");
      _mergeResult = serializedObject.FindProperty("<MergeResult>k__BackingField");
    }

    protected override void DrawInspector()
    {
      // ── Basic Fields ──────────────────────────────────────────────────────
      EditorGUILayout.PropertyField(_itemId, new GUIContent("Item Id"));
      EditorGUILayout.PropertyField(_level, new GUIContent("Level"));
      EditorGUILayout.PropertyField(_icon, new GUIContent("Icon (Addressable)"));
      EditorGUILayout.PropertyField(_itemColor, new GUIContent("Item Color"));
      EditorGUILayout.PropertyField(_mergeResult, new GUIContent("Merge Result"));

      EditorGUILayout.Space(8);

      // ── Shape ─────────────────────────────────────────────────────────────
      EditorGUILayout.LabelField("Shape", EditorStyles.boldLabel);
      EditorGUILayout.HelpBox(
        "Click on a cell to add/remove it from the shape.\n" +
        "First cell (0,0) — top-left corner of bounding box.",
        MessageType.Info);

      DrawShapeGrid();

      EditorGUILayout.Space(4);
      EditorGUILayout.PropertyField(_shape, new GUIContent("Raw Shape (Vector2Int list)"), true);
    }

    private void DrawShapeGrid()
    {
      var cfg = (ItemConfig)target;
      var shape = cfg.Shape ?? new List<Vector2Int>();
      var bounds = cfg.GetBoundsSize();

      // Draw grid at least 3×3 for convenient editing
      int cols = Mathf.Clamp(bounds.x + 1, 3, MaxPreview);
      int rows = Mathf.Clamp(bounds.y + 1, 3, MaxPreview);

      var step = CellSize + CellPad;
      var totalW = cols * step - CellPad;
      var totalH = rows * step - CellPad;
      var startRect = GUILayoutUtility.GetRect(totalW, totalH);

      var filledColor = cfg.ItemColor == default ? new Color(0.35f, 0.55f, 1f, 0.9f) : cfg.ItemColor;
      var emptyColor = new Color(0.18f, 0.18f, 0.18f, 0.7f);
      var borderColor = new Color(0.08f, 0.08f, 0.08f, 1f);

      for (int y = 0; y < rows; y++)
        for (int x = 0; x < cols; x++)
        {
          var coord = new Vector2Int(x, y);
          var isFilled = shape.Contains(coord);

          var rect = new Rect(
            startRect.x + x * step,
            startRect.y + y * step,
            CellSize, CellSize);

          // Border
          EditorGUI.DrawRect(rect, borderColor);
          // Fill
          var innerRect = new Rect(rect.x + 1, rect.y + 1, rect.width - 2, rect.height - 2);
          EditorGUI.DrawRect(innerRect, isFilled ? filledColor : emptyColor);

          // Coordinates (small text)
          var labelStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
          {
            fontSize = 8,
            alignment = TextAnchor.LowerRight,
            normal = { textColor = new Color(1, 1, 1, 0.35f) }
          };
          EditorGUI.LabelField(innerRect, $"{x},{y}", labelStyle);

          // Click — toggle cell
          if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
          {
            bool found = false;
            for (int i = 0; i < _shape.arraySize; i++)
            {
              var el = _shape.GetArrayElementAtIndex(i);
              if (el.vector2IntValue == coord)
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

      // Grid needs constant repaint on hover
      if (Event.current.type == EventType.MouseMove)
        Repaint();
    }
  }
}
