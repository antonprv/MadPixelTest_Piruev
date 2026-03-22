// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Overlays;

namespace Code.Editor.Tools.SceneSwitcher
{
  [Overlay(typeof(SceneView), "Scene Switcher", true)]
  public sealed class SceneSwitcherOverlay : ToolbarOverlay
  {
    public SceneSwitcherOverlay()
      : base(SceneSwitcherDropdown.ID)
    {
    }
  }
}
#endif
