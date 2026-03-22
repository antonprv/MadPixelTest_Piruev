// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using UnityEngine;

namespace Code.Common.CustomTypes.Domain.Collections
{
  [System.Serializable]
  public class PairData<TKey, TValue>
  {
    [SerializeField] private TKey key;
    [SerializeField] private TValue value;

    public TKey Key
    {
      get => key;
      set => key = value;
    }

    public TValue Value
    {
      get => this.value;
      set => this.value = value;
    }

    public PairData() { }

    public PairData(TKey key, TValue value)
    {
      this.key = key;
      this.value = value;
    }
  }
}
