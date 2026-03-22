// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections;

using UnityEngine;

namespace Code.Common.Extensions.Async
{
  public interface ICoroutineRunner
  {
    Coroutine StartCoroutine(IEnumerator load);
    void StopCoroutine(Coroutine coroutine);
  }
}
