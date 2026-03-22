// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Cysharp.Threading.Tasks
{
  /// <summary>
  /// Extension methods that allow awaiting nullable UniTask and UniTask&lt;T&gt; directly.
  /// This enables the null-conditional operator (?.) with async methods:
  /// <code>
  /// await _service?.DoSomethingAsync();         // UniTask?
  /// var result = await _repo?.GetDataAsync();   // UniTask&lt;T&gt;? returns default(T) when null
  /// </code>
  /// </summary>
  public static class UniTaskExtensions
  {
    /// <summary>
    /// Allows awaiting a nullable UniTask (UniTask?).
    /// If the value is null, the await completes immediately (no-op).
    /// </summary>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UniTask.Awaiter GetAwaiter(this UniTask? task)
    {
      return (task ?? UniTask.CompletedTask).GetAwaiter();
    }

    /// <summary>
    /// Allows awaiting a nullable UniTask&lt;T&gt; (UniTask&lt;T&gt;?).
    /// If the value is null, the await completes immediately and returns default(T).
    /// </summary>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UniTask<T>.Awaiter GetAwaiter<T>(this UniTask<T>? task)
    {
      return (task ?? UniTask.FromResult(default(T))).GetAwaiter();
    }
  }
}
