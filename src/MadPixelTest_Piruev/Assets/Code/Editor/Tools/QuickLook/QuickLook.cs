// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System;
using System.Collections.Generic;

using Code.Editor.Tools.QuickLook.Data;

using UnityEditor;

using UnityEngine;

namespace Code.Editor.Tools.QuickLook
{
  public class QuickLook : EditorWindow
  {
    #region Configuration Constants
    private const int MaxColumns = 5;
    private const float MinButtonWidth = 60f;
    private const float SpacingBetweenButtons = 1f;
    private const float WindowEdgePadding = 8f;
    private const float ButtonHeight = 30f;
    #endregion

    #region Fields
    private List<GameObject> _prefabs = new List<GameObject>();
    private List<UnityEngine.ScriptableObject> _scriptableObjects = new List<UnityEngine.ScriptableObject>();

    private int _selectedIndex = 0;
    private Vector2 _scrollPosition = Vector2.zero;
    private QuickLookStaticData _quickLookData;
    #endregion

    #region Unity Menu
    [MenuItem("Window/Quick Look")]
    public static void ShowWindow()
    {
      GetWindow(typeof(QuickLook));
    }
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
      LoadStaticData();
    }

    void OnGUI()
    {
      HandleDragAndDrop();

      _scrollPosition = EditorGUILayout.BeginScrollView(
        _scrollPosition,
        alwaysShowHorizontal: false,
        alwaysShowVertical: false,
        horizontalScrollbar: GUIStyle.none,
        verticalScrollbar: GUI.skin.verticalScrollbar,
        background: GUI.skin.scrollView
      );

      RenderPrefabsList();
      RenderScriptableObjectsList();
      EditorGUILayout.EndScrollView();

      RenderBottomToolbar();
      HandleObjectPickerClosed();
    }
    #endregion

    #region Drag and Drop
    private void HandleDragAndDrop()
    {
      Event currentEvent = Event.current;

      if (currentEvent.type == EventType.DragUpdated || currentEvent.type == EventType.DragPerform)
      {
        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

        if (currentEvent.type == EventType.DragPerform)
        {
          DragAndDrop.AcceptDrag();

          foreach (UnityEngine.Object draggedObject in DragAndDrop.objectReferences)
          {
            if (draggedObject is GameObject gameObject)
              AddPrefab(gameObject);

            if (draggedObject is UnityEngine.ScriptableObject scriptableObject)
              AddScriptableObject(scriptableObject);
          }

          Repaint();
        }
      }
    }
    #endregion

    #region Rendering
    private void RenderPrefabsList()
    {
      if (_prefabs.Count == 0)
        return;

      DrawHeader("Prefabs");
      RenderButtonGrid(_prefabs.Count, (index) =>
      {
        bool isSelected = (_selectedIndex == index);
        DrawButton(_prefabs[index].name, isSelected, () => SelectPrefab(index));
      });
    }

    private void RenderScriptableObjectsList()
    {
      if (_scriptableObjects.Count == 0)
        return;

      DrawHeader("Scriptable Objects");
      RenderButtonGrid(_scriptableObjects.Count, (index) =>
      {
        bool isSelected = (_selectedIndex == index + _prefabs.Count);
        DrawButton(_scriptableObjects[index].name, isSelected, () => SelectScriptableObject(index));
      });
    }

    private void RenderButtonGrid(int itemCount, Action<int> drawItemCallback)
    {
      int columnsCount = CalculateColumnsCountBasedOnWindowWidth();
      float buttonWidth = CalculateButtonWidth(columnsCount);
      int rowCount = Mathf.CeilToInt((float)itemCount / columnsCount);

      for (int row = 0; row < rowCount; row++)
      {
        EditorGUILayout.BeginHorizontal();

        // Left padding
        GUILayout.Space(WindowEdgePadding);

        for (int col = 0; col < columnsCount; col++)
        {
          int index = row * columnsCount + col;
          if (index >= itemCount)
            break;

          drawItemCallback(index);

          // In-between padding
          if (col < columnsCount - 1)
          {
            GUILayout.Space(SpacingBetweenButtons);
          }
        }

        // Right padding
        GUILayout.Space(WindowEdgePadding);

        EditorGUILayout.EndHorizontal();
      }
    }

    private void DrawButton(string label, bool isSelected, Action onClick)
    {
      Color originalColor = GUI.color;
      GUI.color = isSelected ? Color.beige : Color.white;

      float buttonWidth = CalculateButtonWidth(CalculateColumnsCountBasedOnWindowWidth());
      if (GUILayout.Button(label, GUILayout.Width(buttonWidth), GUILayout.Height(ButtonHeight)))
      {
        onClick?.Invoke();
      }

      GUI.color = originalColor;
    }

    private void DrawHeader(string title)
    {
      EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
    }

    private void RenderBottomToolbar()
    {
      EditorGUILayout.BeginHorizontal();

      if (GUILayout.Button("Clear All", EditorStyles.miniButtonLeft))
        ClearLists();

      if (GUILayout.Button("Load File", EditorStyles.miniButtonMid))
        LoadStaticData();

      if (GUILayout.Button("Save File", EditorStyles.miniButtonMid))
        SaveToStaticData();

      if (GUILayout.Button("Add New", EditorStyles.miniButtonRight))
      {
        EditorGUIUtility.ShowObjectPicker<GameObject>(
          null,
          false,
          "t:GameObject",
          GUIUtility.GetControlID(FocusType.Passive)
        );
      }

      EditorGUILayout.EndHorizontal();
    }

    private void HandleObjectPickerClosed()
    {
      if (Event.current.commandName == "ObjectSelectorClosed")
      {
        var selectedObj = EditorGUIUtility.GetObjectPickerObject();
        if (selectedObj is GameObject gameObject)
          AddPrefab(gameObject);
      }
    }
    #endregion

    #region Layout Calculations
    private float CalculateButtonWidth(int columnsCount)
    {
      // Available width = full window width minus edge paddings and vertical scrollbar
      float scrollbarWidth = 15f; // Approximate vertical scrollbar width
      float availableWidth = position.width - (WindowEdgePadding * 2) - scrollbarWidth;

      // Total spacing between buttons
      float totalSpacing = (columnsCount - 1) * SpacingBetweenButtons;

      // Width available for the actual buttons
      float widthForButtons = availableWidth - totalSpacing;

      // Width per single button
      return Mathf.Max(widthForButtons / columnsCount, MinButtonWidth);
    }

    private int CalculateColumnsCountBasedOnWindowWidth()
    {
      // Available window width (accounting for scrollbar)
      float scrollbarWidth = 15f;
      float availableWidth = position.width - (WindowEdgePadding * 2) - scrollbarWidth;

      // Start with max column count and reduce until buttons fit
      for (int columns = MaxColumns; columns >= 1; columns--)
      {
        float totalSpacing = (columns - 1) * SpacingBetweenButtons;
        float widthPerButton = (availableWidth - totalSpacing) / columns;

        // If buttons are wide enough, use this number of columns
        if (widthPerButton >= MinButtonWidth)
        {
          return columns;
        }
      }

      // Fallback to a single column
      return 1;
    }
    #endregion

    #region Selection
    private void SelectPrefab(int index)
    {
      _selectedIndex = index;
      Selection.activeObject = _prefabs[index];
    }

    private void SelectScriptableObject(int index)
    {
      _selectedIndex = index + _prefabs.Count;
      Selection.activeObject = _scriptableObjects[index];
    }
    #endregion

    #region Data Management
    private void AddPrefab(GameObject obj)
    {
      if (!_prefabs.Contains(obj))
      {
        _prefabs.Add(obj);
        Repaint();
      }
    }

    private void AddScriptableObject(UnityEngine.ScriptableObject ScriptableObject)
    {
      if (!_scriptableObjects.Contains(ScriptableObject))
      {
        _scriptableObjects.Add(ScriptableObject);
        Repaint();
      }
    }

    private void ClearLists()
    {
      _prefabs.Clear();
      _scriptableObjects.Clear();
      _selectedIndex = 0;
      Repaint();
    }

    private void LoadStaticData()
    {
      _quickLookData = Resources.Load<QuickLookStaticData>("Editor/QuickLook/QuickLookStaticData");
      if (!_quickLookData)
        return;

      if (_quickLookData.Prefabs.Count > 0)
        _prefabs = new List<GameObject>(_quickLookData.Prefabs);

      if (_quickLookData.ScriptableObjects.Count > 0)
        _scriptableObjects = new List<UnityEngine.ScriptableObject>(_quickLookData.ScriptableObjects);

      Repaint();
    }

    private void SaveToStaticData()
    {
      _quickLookData = Resources.Load<QuickLookStaticData>("Editor/QuickLook/QuickLookStaticData");
      if (!_quickLookData)
        return;

      _quickLookData.Prefabs = _prefabs;
      _quickLookData.ScriptableObjects = _scriptableObjects;

      EditorUtility.SetDirty(_quickLookData);
      AssetDatabase.SaveAssets();
    }
    #endregion
  }
}
