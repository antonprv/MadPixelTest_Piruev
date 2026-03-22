// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using UnityEditor;

namespace Reflex.Editor.DebuggingWindow
{
  public static class ReflexEditorSettings
  {
    public static bool ShowInternalBindings
    {
      get => EditorPrefs.GetBool("ShowInternalBindings", defaultValue: true);
      set => EditorPrefs.SetBool("ShowInternalBindings", value);
    }

    public static bool ShowInheritedBindings
    {
      get => EditorPrefs.GetBool("ShowInheritedBindings", defaultValue: true);
      set => EditorPrefs.SetBool("ShowInheritedBindings", value);
    }
  }
}