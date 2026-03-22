// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using UnityEditor;

using UnityEngine;

namespace Reflex.Editor.DebuggingWindow
{
  internal static class ZenjexStyles
  {
    private static GUIStyle _richLabel;
    public static GUIStyle RichLabel
    {
      get { return _richLabel ??= new GUIStyle(EditorStyles.label) { richText = true }; }
    }

    private static GUIStyle _rowEven;
    public static GUIStyle RowEven
    {
      get { return _rowEven ??= new GUIStyle("CN EntryBackEven") { border = new RectOffset(0, 0, 0, 0) }; }
    }

    private static GUIStyle _rowOdd;
    public static GUIStyle RowOdd
    {
      get { return _rowOdd ??= new GUIStyle("CN EntryBackOdd") { border = new RectOffset(0, 0, 0, 0) }; }
    }

    private static GUIStyle _rowSelected;
    public static GUIStyle RowSelected
    {
      get { return _rowSelected ??= new GUIStyle("MeTransitionSelectHead") { border = new RectOffset(0, 0, 0, 0) }; }
    }

    private static GUIStyle _statusGreen;
    public static GUIStyle StatusGreen
    {
      get
      {
        return _statusGreen ??= new GUIStyle(EditorStyles.miniLabel)
        {
          fontStyle = FontStyle.Bold,
          normal = { textColor = new Color(0.3f, 0.9f, 0.4f) }
        };
      }
    }

    private static GUIStyle _statusGray;
    public static GUIStyle StatusGray
    {
      get
      {
        return _statusGray ??= new GUIStyle(EditorStyles.miniLabel)
        {
          fontStyle = FontStyle.Bold,
          normal = { textColor = new Color(0.5f, 0.5f, 0.5f) }
        };
      }
    }

    private static GUIStyle _statusWarning;
    public static GUIStyle StatusWarning
    {
      get
      {
        return _statusWarning ??= new GUIStyle(EditorStyles.miniLabel)
        {
          normal = { textColor = new Color(1f, 0.7f, 0.2f) }
        };
      }
    }

    public static readonly Color NormalColor = new Color(0.85f, 0.85f, 0.85f);
    public static readonly Color LateColor = new Color(1.0f, 0.65f, 0.2f);
  }
}
