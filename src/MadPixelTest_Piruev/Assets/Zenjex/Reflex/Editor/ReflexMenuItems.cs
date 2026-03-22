// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System.IO;

using Reflex.Configuration;
using Reflex.Core;
using Reflex.Editor.DebuggingWindow;

using UnityEditor;
using UnityEditor.SceneManagement;

using UnityEngine;

namespace Reflex.Editor
{
  internal static class ReflexMenuItems
  {
    [MenuItem("Window/Analysis/Reflex Debugger %e")]
    private static void OpenReflexDebuggingWindow()
    {
      EditorWindow.GetWindow<ReflexDebuggerWindow>(false, "Reflex Debugger", true);
    }

    [MenuItem("Assets/Create/Reflex/Settings")]
    private static void CreateReflexSettings()
    {
      var directory = UnityEditorUtility.GetSelectedPathInProjectWindow();
      var desiredAssetPath = Path.Combine(directory, "ReflexSettings.asset");
      UnityEditorUtility.CreateScriptableObject<ReflexSettings>(desiredAssetPath);
    }

    [MenuItem("Assets/Create/Reflex/RootScope")]
    private static void CreateReflexRootScope()
    {
      var directory = UnityEditorUtility.GetSelectedPathInProjectWindow();
      var desiredAssetPath = Path.Combine(directory, "RootScope.prefab");

      void Edit(GameObject prefab)
      {
        prefab.AddComponent<ContainerScope>();
      }

      UnityEditorUtility.CreatePrefab(desiredAssetPath, Edit);
    }

    [MenuItem("GameObject/Reflex/SceneScope")]
    private static void CreateReflexSceneScope()
    {
      var containerScope = new GameObject("SceneScope").AddComponent<ContainerScope>();
      Selection.activeObject = containerScope.gameObject;
      EditorSceneManager.MarkSceneDirty(containerScope.gameObject.scene);
    }
  }
}