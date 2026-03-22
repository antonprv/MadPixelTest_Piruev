// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System;
using System.Threading;

namespace Cysharp.Threading.Tasks
{
  public static partial class UnityAsyncExtensions
  {
    public static UniTask StartAsyncCoroutine(this UnityEngine.MonoBehaviour monoBehaviour, Func<CancellationToken, UniTask> asyncCoroutine)
    {
      var token = monoBehaviour.GetCancellationTokenOnDestroy();
      return asyncCoroutine(token);
    }
  }
}