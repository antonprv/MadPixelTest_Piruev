// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

#if UNITY_EDITOR
using UnityEditor;

using UnityEngine;

namespace Code.Editor.Common.Manifests.Interfaces
{
  #region Custom Key Drawer Support

  /// <summary>
  /// Interface for custom key drawers that can render dictionary keys
  /// with specialized UI controls (dropdowns, object pickers, etc.).
  /// </summary>
  public interface ICustomKeyDrawer
  {
    /// <summary>
    /// Draws the dictionary with custom key rendering.
    /// </summary>
    void DrawDictionaryWithCustomKeys(SerializedProperty property, GUIContent label);

    /// <summary>
    /// Clears any cached data.
    /// </summary>
    void ClearCache();
  }

  #endregion
}
#endif
