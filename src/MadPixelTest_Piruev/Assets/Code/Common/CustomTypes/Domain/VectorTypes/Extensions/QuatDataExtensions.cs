// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Common.Domain.DataTypes;
using Code.Common.FastMath;


namespace Code.Common.Domain.VectorTypes.Extensions
{
  /// <summary>
  /// Mathematical operations for QuatData
  /// No Unity dependencies
  /// </summary>
  public static partial class QuatDataExtensions
  {
    // -----------------------------
    // Quaternion interpolation
    // -----------------------------

    /// <summary>
    /// Spherical linear interpolation (simplified)
    /// </summary>
    public static QuatData Slerp(QuatData a, QuatData b, float t)
    {
      t = FMath.Clamp01(t);
      return new QuatData(
          FMath.Lerp(a.X, b.X, t),
          FMath.Lerp(a.Y, b.Y, t),
          FMath.Lerp(a.Z, b.Z, t),
          FMath.Lerp(a.W, b.W, t)
      );
    }

    /// <summary>
    /// Linear interpolation (component-wise)
    /// </summary>
    public static QuatData Lerp(QuatData a, QuatData b, float t)
    {
      t = FMath.Clamp01(t);
      return new QuatData(
          FMath.Lerp(a.X, b.X, t),
          FMath.Lerp(a.Y, b.Y, t),
          FMath.Lerp(a.Z, b.Z, t),
          FMath.Lerp(a.W, b.W, t)
      );
    }

    // -----------------------------
    // Basic arithmetic
    // -----------------------------

    public static QuatData Add(this QuatData a, QuatData b) =>
        new(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);

    public static QuatData Subtract(this QuatData a, QuatData b) =>
        new(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);

    public static QuatData Multiply(this QuatData q, float scalar) =>
        new(q.X * scalar, q.Y * scalar, q.Z * scalar, q.W * scalar);

    public static QuatData Divide(this QuatData q, float scalar, float epsilon = FMath.KINDA_SMALL_NUMBER)
    {
      if (FMath.Abs(scalar) < epsilon)
        throw new System.DivideByZeroException("Cannot divide quaternion by zero");

      return new QuatData(q.X / scalar, q.Y / scalar, q.Z / scalar, q.W / scalar);
    }

    public static QuatData Negate(this QuatData q) => new(-q.X, -q.Y, -q.Z, -q.W);

    // -----------------------------
    // Dot product
    // -----------------------------

    public static float Dot(this QuatData a, QuatData b) =>
        a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;

    // -----------------------------
    // Magnitude / Normalization
    // -----------------------------

    public static float SqrMagnitude(this QuatData q) =>
        q.X * q.X + q.Y * q.Y + q.Z * q.Z + q.W * q.W;

    public static float Magnitude(this QuatData q) =>
        FMath.FastSqrt(q.SqrMagnitude());

    public static QuatData Normalized(this QuatData q, float epsilon = FMath.KINDA_SMALL_NUMBER)
    {
      float x = q.X;
      float y = q.Y;
      float z = q.Z;
      float w = q.W;

      float sqrMag = x * x + y * y + z * z + w * w;

      if (sqrMag < epsilon * epsilon)
        return QuatData.Identity;

      float invMag = FMath.FastInvSqrt(sqrMag);
      x *= invMag;
      y *= invMag;
      z *= invMag;
      w *= invMag;

      return new QuatData(x, y, z, w);
    }

    // -----------------------------
    // Component-wise operations
    // -----------------------------

    public static QuatData WithX(this QuatData q, float newX) =>
        new(newX, q.Y, q.Z, q.W);

    public static QuatData WithY(this QuatData q, float newY) =>
        new(q.X, newY, q.Z, q.W);

    public static QuatData WithZ(this QuatData q, float newZ) =>
        new(q.X, q.Y, newZ, q.W);

    public static QuatData WithW(this QuatData q, float newW) =>
        new(q.X, q.Y, q.Z, newW);

    public static QuatData Abs(this QuatData q) =>
        new(FMath.Abs(q.X), FMath.Abs(q.Y), FMath.Abs(q.Z), FMath.Abs(q.W));

    // -----------------------------
    // Comparison
    // -----------------------------

    public static bool IsNearlyZero(this QuatData q, float epsilon = FMath.KINDA_SMALL_NUMBER) =>
        q.SqrMagnitude() <= epsilon * epsilon;

    public static bool NearlyEqual(this QuatData a, QuatData b, float epsilon = FMath.KINDA_SMALL_NUMBER) =>
        FMath.IsNearlyEqual(a.X, b.X, epsilon)
        && FMath.IsNearlyEqual(a.Y, b.Y, epsilon)
        && FMath.IsNearlyEqual(a.Z, b.Z, epsilon)
        && FMath.IsNearlyEqual(a.W, b.W, epsilon);
  }
}
