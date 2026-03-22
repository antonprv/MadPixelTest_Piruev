// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.
using System.Collections.Generic;

using UnityEngine;

namespace Code.Common.CustomTypes.Domain.Collections
{
  [System.Serializable]
  public class HashSetData<T> : HashSet<T>, ISerializationCallbackReceiver
  {
    [SerializeField, HideInInspector]
    private List<T> data = new List<T>();

    public void OnAfterDeserialize()
    {
      Clear();
      foreach (var item in data)
      {
        Add(item);
      }
    }

    public void OnBeforeSerialize()
    {
      data.Clear();
      foreach (var item in this)
      {
        data.Add(item);
      }
    }
  }
}
