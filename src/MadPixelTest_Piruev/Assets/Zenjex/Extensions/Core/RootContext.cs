// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Reflex.Core;

namespace Zenjex.Extensions.Core
{
  public static class RootContext
  {
    /// <summary>
    /// Use this for post-initialization binding
    /// </summary>
    public static Container Runtime => ProjectRootInstaller.RootContainer;

    /// <summary>
    /// Check if a global RootContainer has been created
    /// </summary>
    public static bool HasInstance => ProjectRootInstaller.RootContainer != null;

    /// <summary>
    /// Resolve dependencies from RootContainer
    /// </summary>
    public static T Resolve<T>()
    {
      var container = ProjectRootInstaller.RootContainer;
      if (container == null)
        throw new System.InvalidOperationException(
          "RootContainer is not created yet. Ensure ProjectRootInstaller is initialized.");

      return container.Resolve<T>();
    }

    /// <summary>e
    /// Resolve dependencies by type
    /// </summary>
    public static object Resolve(System.Type type)
    {
      var container = ProjectRootInstaller.RootContainer;
      if (container == null)
        throw new System.InvalidOperationException(
          "RootContainer is not created yet. Ensure ProjectRootInstaller is initialized.");

      return container.Resolve(type);
    }
  }
}
