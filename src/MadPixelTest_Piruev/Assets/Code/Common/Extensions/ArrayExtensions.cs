// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System;

namespace Code.Common.Extensions
{
  public static class ArrayExtensions
  {
    public static void Empty<T>(this T[] array) where T : class
    {
      if (array == null)
        return;

      Array.Clear(array, 0, array.Length);
    }
  }
}
