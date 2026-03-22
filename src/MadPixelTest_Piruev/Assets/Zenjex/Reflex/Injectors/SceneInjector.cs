// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Reflex.Core;

using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

namespace Reflex.Injectors
{
  internal static class SceneInjector
  {
    internal static void Inject(Scene scene, Container container)
    {
      using var pooledObject1 = ListPool<GameObject>.Get(out var rootGameObjects);
      scene.GetRootGameObjects(rootGameObjects);
      GameObjectInjector.InjectRecursiveMany(rootGameObjects, container);
    }
  }
}