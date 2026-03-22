using UnityEngine;
using UnityEditor;
using BagFight.Data;

namespace BagFight.Editor
{
  /// <summary>
  /// Инспектор для BagConfig — показывает превью формы сумки.
  /// </summary>
  [CustomEditor(typeof(BagConfig))]
  public class BagConfigEditor : UnityEditor.Editor
  {
    private const int CellSize = 18;
    private const int CellPad  = 2;

    public override void OnInspectorGUI()
    {
      DrawDefaultInspector();

      EditorGUILayout.Space(8);
      EditorGUILayout.LabelField("Shape preview", EditorStyles.boldLabel);

      var cfg     = (BagConfig)target;
      var active  = cfg.GetActiveCellsSet();
      var gridSz  = cfg.GridSize;

      int step   = CellSize + CellPad;
      int totalW = gridSz.x * step;
      int totalH = gridSz.y * step;

      var startRect = GUILayoutUtility.GetRect(totalW, totalH);

      var activeColor   = new Color(0.25f, 0.65f, 0.35f, 0.9f);
      var inactiveColor = new Color(0.1f,  0.1f,  0.1f,  0.5f);
      var borderColor   = new Color(0.05f, 0.05f, 0.05f, 1f);

      for (int y = 0; y < gridSz.y; y++)
      for (int x = 0; x < gridSz.x; x++)
      {
        var coord = new Vector2Int(x, y);
        var rect  = new Rect(
          startRect.x + x * step,
          startRect.y + y * step,
          CellSize, CellSize);

        EditorGUI.DrawRect(rect, borderColor);
        var inner = new Rect(rect.x + 1, rect.y + 1, rect.width - 2, rect.height - 2);
        EditorGUI.DrawRect(inner, active.Contains(coord) ? activeColor : inactiveColor);
      }
    }
  }
}
