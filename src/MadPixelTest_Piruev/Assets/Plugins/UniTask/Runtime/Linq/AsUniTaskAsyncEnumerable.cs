// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

namespace Cysharp.Threading.Tasks.Linq
{
  public static partial class UniTaskAsyncEnumerable
  {
    public static IUniTaskAsyncEnumerable<TSource> AsUniTaskAsyncEnumerable<TSource>(this IUniTaskAsyncEnumerable<TSource> source)
    {
      return source;
    }
  }
}
