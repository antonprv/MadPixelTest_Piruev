// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using UnityEditor;

using UnityEngine;

namespace Code.Editor.Common
{
  public abstract class ManualSaveEditor : UnityEditor.Editor
  {
    private bool _hasUnsavedChanges;

    public sealed override void OnInspectorGUI()
    {
      serializedObject.Update();

      EditorGUI.BeginChangeCheck();

      DrawInspector();

      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(target, $"Modify {target.GetType().Name}");
        _hasUnsavedChanges = true;
      }

      serializedObject.ApplyModifiedProperties();

      EditorGUILayout.Space(8f);

      DrawUnsavedWarning();
      DrawSaveButton();
    }

    protected abstract void DrawInspector();

    protected virtual void OnDisable()
    {
      if (!_hasUnsavedChanges)
        return;

      bool save =
        EditorUtility.DisplayDialog(
          "Unsaved changes",
          $"{target.GetType().Name} has unsaved changes. Do you want to save them?",
          "Save",
          "Discard");

      if (save)
      {
        Save();
      }
      else
      {
        _hasUnsavedChanges = false;
      }
    }

    protected void DrawDefaultInspectorWithManualSave()
    {
      DrawDefaultInspector();
    }

    private void DrawUnsavedWarning()
    {
      if (_hasUnsavedChanges)
      {
        EditorGUILayout.Space(6);

        EditorGUILayout.HelpBox(
          "There are unsaved changes",
          MessageType.Warning);
      }
    }

    private void DrawSaveButton()
    {
      EditorGUILayout.Space(4);

      using (new EditorGUI.DisabledScope(!_hasUnsavedChanges))
      {
        GUI.backgroundColor = _hasUnsavedChanges ? Color.yellow : Color.gray;

        if (GUILayout.Button("Save", GUILayout.Height(30)))
        {
          Save();
        }

        GUI.backgroundColor = Color.white;
      }
    }

    private void Save()
    {
      EditorUtility.SetDirty(target);
      AssetDatabase.SaveAssets();
      AssetDatabase.Refresh();

      _hasUnsavedChanges = false;
    }
  }
}
