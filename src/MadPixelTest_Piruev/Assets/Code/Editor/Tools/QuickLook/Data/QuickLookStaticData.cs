// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

using UnityEngine;

namespace Code.Editor.Tools.QuickLook.Data
{
  [UnityEngine.CreateAssetMenu(fileName = "QuickLookStaticData",
  menuName = "StaticData/Editor/QuickLookStaticData")]
  public class QuickLookStaticData : UnityEngine.ScriptableObject
  {
    public List<GameObject> Prefabs;
    public List<UnityEngine.ScriptableObject> ScriptableObjects;
  }
}
