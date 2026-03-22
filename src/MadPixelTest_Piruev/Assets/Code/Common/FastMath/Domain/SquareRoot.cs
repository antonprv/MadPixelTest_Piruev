// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System.Runtime.CompilerServices;

namespace Code.Common.FastMath
{
  /// <summary>
  /// Fast math operations optimized for game development
  /// Uses techniques from Quake III Arena and other game engines
  /// </summary>
  public static partial class FMath
  {
    /// <summary>
    /// Legendary Fast Inverse Square Root from Quake III Arena
    /// Computes 1/sqrt(x) faster than standard Math.Sqrt
    ///
    /// Original comment from Quake III:
    /// "What the fuck?"
    /// </summary>
    /// <param name="x">Number to compute inverse square root of</param>
    /// <returns>Approximate value of 1/sqrt(x)</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float FastInvSqrt(float x, bool prescise = false)
    {
      // Magic number from Quake III
      const int MAGIC_NUMBER = 0x5f3759df;

      float xhalf = 0.5f * x;
      int i = FloatToInt32Bits(x);           // Evil floating point bit-level hack
      i = MAGIC_NUMBER - (i >> 1);           // First magic step
      x = Int32BitsToFloat(i);               // Convert back to float
      x = x * (1.5f - xhalf * x * x);        // One Newton-Raphson iteration

      if (prescise)
        x = x * (1.5f - xhalf * x * x);

      return x;
    }

    /// <summary>
    /// Fast square root using Fast Inverse Square Root
    /// sqrt(x) = x * (1/sqrt(x))
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float FastSqrt(float x)
    {
      if (x <= 0f)
        return 0f;

      return x * FastInvSqrt(x);
    }

    /// <summary>
    /// Fast vector normalization using FastInvSqrt
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FastNormalize(ref float x, ref float y, ref float z, float epsilon = SMALL_NUMBER)
    {
      float lengthSquared = x * x + y * y + z * z;

      if (lengthSquared < epsilon)
      {
        x = y = z = 0f;
        return;
      }

      float invLength = FastInvSqrt(lengthSquared);
      x *= invLength;
      y *= invLength;
      z *= invLength;
    }

    /// <summary>
    /// Fast computation of vector length
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float FastLength(float x, float y, float z, float epsilon = SMALL_NUMBER)
    {
      float lengthSquared = x * x + y * y + z * z;

      if (lengthSquared < epsilon)
        return 0f;

      return lengthSquared * FastInvSqrt(lengthSquared);
    }

    /// <summary>
    /// Fast distance calculation between two points
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float FastDistance(float x1, float y1, float z1, float x2, float y2, float z2)
    {
      float dx = x2 - x1;
      float dy = y2 - y1;
      float dz = z2 - z1;

      return FastLength(dx, dy, dz);
    }

    /// <summary>
    /// Squared distance (fastest way for comparisons)
    /// Use this for checks like "is closer than X" - no need for sqrt at all!
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DistanceSquared(float x1, float y1, float z1, float x2, float y2, float z2)
    {
      float dx = x2 - x1;
      float dy = y2 - y1;
      float dz = z2 - z1;

      return dx * dx + dy * dy + dz * dz;
    }

    // Helper methods for bit manipulation with floats

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe int FloatToInt32Bits(float value)
    {
      return *(int*)&value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe float Int32BitsToFloat(int value)
    {
      return *(float*)&value;
    }
  }
}
