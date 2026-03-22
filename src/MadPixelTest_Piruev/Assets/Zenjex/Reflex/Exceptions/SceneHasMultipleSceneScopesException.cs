// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System;

using UnityEngine.SceneManagement;

namespace Reflex.Exceptions
{
  public class SceneHasMultipleSceneScopesException : Exception
  {
    public SceneHasMultipleSceneScopesException(Scene scene) : base(GenerateMessage(scene))
    {
    }

    private static string GenerateMessage(Scene scene)
    {
      return $"Scene '{scene.name}' has multiple SceneScope instances. None or only one SceneScope is allowed per scene.";
    }
  }
}