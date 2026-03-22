// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System.Runtime.CompilerServices;

namespace Code.Common.FastMath
{
  /// <summary>
  /// Extension methods for convenient usage
  /// </summary>
  public static class FastMathExtensions
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float FastSqrt(this float value) =>
      FMath.FastSqrt(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float FastInvSqrt(this float value) =>
      FMath.FastInvSqrt(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNearlyEqual(
      this float value,
      float other,
      float epsilon = FMath.KINDA_SMALL_NUMBER) =>
      FMath.IsNearlyEqual(value, other, epsilon);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNearlyZero(this float value, float epsilon = FMath.KINDA_SMALL_NUMBER) =>
      FMath.IsNearlyZero(value, epsilon);
  }
}
