// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System.Runtime.CompilerServices;

namespace Code.Common.FastMath
{
  public static partial class FMath
  {
    // -----------------------------
    // Clamp (without using Math.*)
    // -----------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Clamp(float value, float min, float max)
    {
      if (value < min) return min;
      if (value > max) return max;
      return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Clamp01(float value)
    {
      if (value < 0f) return 0f;
      if (value > 1f) return 1f;
      return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Clamp(int value, int min, int max)
    {
      if (value < min) return min;
      if (value > max) return max;
      return value;
    }

    // -----------------------------
    // Interpolation
    // -----------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Lerp(float a, float b, float t)
    {
      if (t <= 0f) return a;
      if (t >= 1f) return b;
      return a + (b - a) * t;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float LerpUnclamped(float a, float b, float t) => a + (b - a) * t;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float InverseLerp(float a, float b, float value)
    {
      float diff = b - a;
      if (diff == 0f) return 0f;
      return Clamp01((value - a) / diff);
    }

    // -----------------------------
    // Comparison (without using Math.Abs)
    // -----------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Abs(float value) => value >= 0f ? value : -value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNearlyEqual(float a, float b, float epsilon = KINDA_SMALL_NUMBER)
    {
      float diff = a - b;
      return diff < epsilon && diff > -epsilon;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNearlyZero(float value, float epsilon = KINDA_SMALL_NUMBER) =>
      value < epsilon;

    // -----------------------------
    // Fast floor (less accurate but faster)
    // -----------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Floor(float value)
    {
      int i = (int)value;
      return value < i ? i - 1 : i;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Ceil(float value)
    {
      int i = (int)value;
      return value > i ? i + 1 : i;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Round(float value) => Floor(value + 0.5f);

    // -----------------------------
    // Angle
    // -----------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Repeat(float t, float length) =>
      t - Floor(t / length) * length;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DeltaAngle(float current, float target)
    {
      float delta = Repeat(target - current, 360f);
      if (delta > 180f) delta -= 360f;
      return delta;
    }

    // -----------------------------
    // Sign
    // -----------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Sign(float value) => value >= 0f ? 1f : -1f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int SignInt(float value) => value >= 0f ? 1 : -1;

    // -----------------------------
    // Unsafe Min/Max without allocations
    // -----------------------------

    public static unsafe float Max(System.ReadOnlySpan<float> values)
    {
      if (values.Length == 0)
        throw new System.ArgumentException("Empty span");

      fixed (float* ptr = values)
      {
        float max = ptr[0];
        for (int i = 1; i < values.Length; i++)
        {
          float v = ptr[i];
          if (v > max) max = v;
        }
        return max;
      }
    }

    public static unsafe float Min(System.ReadOnlySpan<float> values)
    {
      if (values.Length == 0)
        throw new System.ArgumentException("Empty span");

      fixed (float* ptr = values)
      {
        float min = ptr[0];
        for (int i = 1; i < values.Length; i++)
        {
          float v = ptr[i];
          if (v < min) min = v;
        }
        return min;
      }
    }

    // -----------------------------
    // Map without extra calls
    // -----------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Map(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
      float t = (value - fromMin) / (fromMax - fromMin);
      return toMin + (toMax - toMin) * t;
    }
  }
}
