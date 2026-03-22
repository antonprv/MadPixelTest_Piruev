// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System;
using System.Collections.Generic;

using UnityEngine;

namespace Code.Common.CustomTypes.Domain.Collections
{
  /// <summary>
  /// Interface for types that support manual serialization triggering.
  /// </summary>
  public interface IForceSerialization
  {
    /// <summary>
    /// Forces synchronization of data to serialized format.
    /// </summary>
    void ForceSerialization();
  }

  /// <summary>
  /// Serializable dictionary for Unity.
  /// Maintains synchronization between dictionary data and serialized lists.
  /// Stable for Editor, Undo, AutoFill and runtime.
  /// </summary>
  [Serializable]
  public class DictionaryData<TKey, TValue>
      : Dictionary<TKey, TValue>,
        ISerializationCallbackReceiver,
        IForceSerialization
  {
    [SerializeField]
    private List<TKey> keyData = new List<TKey>();

    [SerializeField]
    private List<TValue> valueData = new List<TValue>();

    #region ISerializationCallbackReceiver

    /// <summary>
    /// Called by Unity before serialization.
    /// Always synchronizes lists with dictionary.
    /// </summary>
    public void OnBeforeSerialize()
    {
      SynchronizeListsWithDictionary();
    }

    /// <summary>
    /// Called by Unity after deserialization.
    /// Rebuilds dictionary from serialized lists.
    /// </summary>
    public void OnAfterDeserialize()
    {
      RebuildDictionaryFromSerializedData();
    }

    #endregion

    #region Dictionary Overrides

    public new TValue this[TKey key]
    {
      get => base[key];
      set
      {
        base[key] = value;
      }
    }

    public new void Add(TKey key, TValue value)
    {
      base.Add(key, value);
    }

    public new bool Remove(TKey key)
    {
      return base.Remove(key);
    }

    public new void Clear()
    {
      base.Clear();
    }

    public new bool TryAdd(TKey key, TValue value)
    {
#if NET_STANDARD_2_1 || NET_6_0_OR_GREATER
      return base.TryAdd(key, value);
#else
      if (ContainsKey(key))
        return false;

      base.Add(key, value);
      return true;
#endif
    }

    #endregion

    #region Public API

    /// <summary>
    /// Forces synchronization of dictionary data into serialized lists.
    /// Use from Editor code after manual modifications.
    /// </summary>
    public void ForceSerialization()
    {
      SynchronizeListsWithDictionary();
    }

    #endregion

    #region Internal Sync Logic

    private void RebuildDictionaryFromSerializedData()
    {
      base.Clear();

      int count = Mathf.Min(keyData.Count, valueData.Count);

      for (int i = 0; i < count; i++)
      {
        TKey key = keyData[i];

        if (!IsValidKey(key))
          continue;

        if (!ContainsKey(key))
        {
          base[key] = valueData[i];
        }
      }
    }

    private void SynchronizeListsWithDictionary()
    {
      keyData.Clear();
      valueData.Clear();

      foreach (var pair in this)
      {
        keyData.Add(pair.Key);
        valueData.Add(pair.Value);
      }
    }

    private bool IsValidKey(TKey key)
    {
      if (typeof(TKey).IsClass || typeof(TKey).IsInterface)
        return key != null;

      return true;
    }

    #endregion
  }
}
