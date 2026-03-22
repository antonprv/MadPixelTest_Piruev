// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Data.StaticData.Configs;

using Code.Editor.Common;

using UnityEditor;

using UnityEngine;

namespace Code.Editor
{
  /// <summary>
  /// Inspector for BagConfig.
  ///
  /// Shape preview is interactive:
  ///   - Green cell = active (item can be placed here)
  ///   - Grey cell  = inactive (blocked slot)
  ///   - Click a cell to toggle its active state
  ///
  /// Fix: after a cell click we set GUI.changed = true so that
  /// ManualSaveEditor's EndChangeCheck() catches it and marks
  /// _hasUnsavedChanges, enabling the Save button.
  /// </summary>
  [CustomEditor(typeof(BagConfig))]
  public class BagConfigEditor : ManualSaveEditor
  {
    private const int CellSize = 22;
    private const int CellPad = 3;

    private SerializedProperty _activeCellsProp;

    private void OnEnable()
    {
      _activeCellsProp = serializedObject.FindProperty("_activeCells");
    }

    protected override void DrawInspector()
    {
      DrawDefaultInspectorWithManualSave();

      EditorGUILayout.Space(8);
      EditorGUILayout.LabelField("Shape preview", EditorStyles.boldLabel);
      EditorGUILayout.HelpBox(
        "Click a cell to toggle it on/off.\n" +
        "Green = active   |   Grey = inactive (blocked)",
        MessageType.Info);

      DrawShapeGrid();

      if (Event.current.type == EventType.MouseMove ||
          Event.current.type == EventType.MouseDown)
        Repaint();
    }

    private void DrawShapeGrid()
    {
      var cfg = (BagConfig)target;
      var gridSz = cfg.GridSize;
      var active = cfg.GetActiveCellsSet();

      int step = CellSize + CellPad;
      int totalW = gridSz.x * step - CellPad;
      int totalH = gridSz.y * step - CellPad;

      var startRect = GUILayoutUtility.GetRect(totalW, totalH);

      var activeColor = new Color(0.25f, 0.65f, 0.35f, 0.95f);
      var inactiveColor = new Color(0.30f, 0.30f, 0.30f, 0.80f);
      var borderColor = new Color(0.05f, 0.05f, 0.05f, 1f);
      var hoverColor = new Color(1f, 1f, 1f, 0.12f);

      var mousePos = Event.current.mousePosition;

      for (int y = 0; y < gridSz.y; y++)
        for (int x = 0; x < gridSz.x; x++)
        {
          var coord = new Vector2Int(x, y);
          var rect = new Rect(
            startRect.x + x * step,
            startRect.y + y * step,
            CellSize, CellSize);

          bool isActive = active.Contains(coord);
          bool isHover = rect.Contains(mousePos);

          EditorGUI.DrawRect(rect, borderColor);
          var inner = new Rect(rect.x + 1, rect.y + 1,
                               rect.width - 2, rect.height - 2);
          EditorGUI.DrawRect(inner, isActive ? activeColor : inactiveColor);

          if (isHover)
            EditorGUI.DrawRect(inner, hoverColor);

          if (Event.current.type == EventType.MouseDown
              && rect.Contains(Event.current.mousePosition))
          {
            ToggleCell(cfg, coord);

            // KEY FIX: ManualSaveEditor wraps DrawInspector() inside
            // BeginChangeCheck / EndChangeCheck. Explicitly setting
            // GUI.changed = true here ensures EndChangeCheck() returns
            // true → _hasUnsavedChanges = true → Save button activates.
            GUI.changed = true;

            Event.current.Use();
          }
        }
    }

    private void ToggleCell(BagConfig cfg, Vector2Int coord)
    {
      serializedObject.Update();

      // If the list is empty ("all cells active" implicit mode),
      // materialise every cell first, then remove the clicked one.
      if (_activeCellsProp.arraySize == 0)
      {
        var gs = cfg.GridSize;
        for (int fy = 0; fy < gs.y; fy++)
          for (int fx = 0; fx < gs.x; fx++)
            AppendCell(new Vector2Int(fx, fy));
      }

      int foundIndex = -1;
      for (int i = 0; i < _activeCellsProp.arraySize; i++)
      {
        if (_activeCellsProp.GetArrayElementAtIndex(i).vector2IntValue == coord)
        {
          foundIndex = i;
          break;
        }
      }

      if (foundIndex >= 0)
        _activeCellsProp.DeleteArrayElementAtIndex(foundIndex);
      else
        AppendCell(coord);

      serializedObject.ApplyModifiedProperties();
    }

    private void AppendCell(Vector2Int coord)
    {
      int idx = _activeCellsProp.arraySize;
      _activeCellsProp.arraySize++;
      _activeCellsProp.GetArrayElementAtIndex(idx).vector2IntValue = coord;
    }
  }
}
