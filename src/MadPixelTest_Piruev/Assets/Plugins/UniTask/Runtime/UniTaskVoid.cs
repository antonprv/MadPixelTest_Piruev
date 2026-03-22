// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

#pragma warning disable CS1591
#pragma warning disable CS0436

using System.Runtime.CompilerServices;

using Cysharp.Threading.Tasks.CompilerServices;

namespace Cysharp.Threading.Tasks
{
  [AsyncMethodBuilder(typeof(AsyncUniTaskVoidMethodBuilder))]
  public readonly struct UniTaskVoid
  {
    public void Forget()
    {
    }
  }
}

