// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System;

namespace Code.Common.Extensions
{
  public static class FunctionalExtensions
  {
    public static T With<T>(this T self, Action<T> set)
    {
      set.Invoke(self);
      return self;
    }

    public static T With<T>(this T self, Action<T> apply, bool when)
    {
      if (when)
        apply?.Invoke(self);

      return self;
    }
  }
}
