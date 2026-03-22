// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System;
using System.Reflection;

namespace Reflex.Extensions
{
  internal static class MethodInfoExtensions
  {
    public static T CreateDelegate<T>(this MethodInfo methodInfo) where T : Delegate
    {
      return (T)methodInfo.CreateDelegate(typeof(T));
    }
  }
}