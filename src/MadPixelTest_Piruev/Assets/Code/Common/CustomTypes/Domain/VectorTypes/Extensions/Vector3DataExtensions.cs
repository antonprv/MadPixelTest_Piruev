// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Common.Domain.DataTypes;
using Code.Common.FastMath;


namespace Code.Common.Domain.VectorTypes.Extensions
{
  /// <summary>
  /// Mathematical operations for Vector3Data
  /// No Unity dependencies - testable without game engine
  /// </summary>
  public static class Vector3DataExtensions
  {
    // Magnitude and Distance
    public static float Magnitude(this Vector3Data v) =>
        FMath.FastLength(v.X, v.Y, v.Z);

    public static float SqrMagnitude(this Vector3Data v) =>
      v.X * v.X + v.Y * v.Y + v.Z * v.Z;

    public static float Distance(this Vector3Data a, Vector3Data b) =>
      FMath.FastDistance(a.X, a.Y, a.Z, b.X, b.Y, b.Z);


    public static float SqrDistance(this Vector3Data a, Vector3Data b) =>
      FMath.DistanceSquared(a.X, a.Y, a.Z, b.X, b.Y, b.Z);


    // Normalization
    public static Vector3Data Normalized(
      this Vector3Data v,
      float epsilon = FMath.KINDA_SMALL_NUMBER)
    {
      float x = v.X;
      float y = v.Y;
      float z = v.Z;

      FMath.FastNormalize(ref x, ref y, ref z, epsilon);

      return new Vector3Data(x, y, z);
    }


    // Arithmetic Operations
    public static Vector3Data Add(this Vector3Data a, Vector3Data b) =>
        new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    public static Vector3Data Subtract(this Vector3Data a, Vector3Data b) =>
        new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    public static Vector3Data Multiply(this Vector3Data v, float scalar) =>
        new(v.X * scalar, v.Y * scalar, v.Z * scalar);

    public static Vector3Data Divide(this Vector3Data v, float scalar,
        float epsilon = FMath.KINDA_SMALL_NUMBER)
    {
      if (FMath.Abs(scalar) < epsilon)
        throw new System.DivideByZeroException("Cannot divide vector by zero");

      return new Vector3Data(v.X / scalar, v.Y / scalar, v.Z / scalar);
    }

    public static Vector3Data Negate(this Vector3Data v) => new(-v.X, -v.Y, -v.Z);

    // Vector Products
    public static float Dot(this Vector3Data a, Vector3Data b) =>
      a.X * b.X + a.Y * b.Y + a.Z * b.Z;

    public static Vector3Data Cross(this Vector3Data a, Vector3Data b)
    {
      return new Vector3Data(
          a.Y * b.Z - a.Z * b.Y,
          a.Z * b.X - a.X * b.Z,
          a.X * b.Y - a.Y * b.X
      );
    }

    // Component-wise Operations
    public static Vector3Data WithX(this Vector3Data v, float newX) => new(newX, v.Y, v.Z);

    public static Vector3Data WithY(this Vector3Data v, float newY) => new(v.X, newY, v.Z);

    public static Vector3Data WithZ(this Vector3Data v, float newZ) => new(v.X, v.Y, newZ);

    public static Vector3Data AddX(this Vector3Data v, float deltaX) => new(v.X + deltaX, v.Y, v.Z);

    public static Vector3Data AddY(this Vector3Data v, float deltaY) => new(v.X, v.Y + deltaY, v.Z);

    public static Vector3Data AddZ(this Vector3Data v, float deltaZ) => new(v.X, v.Y, v.Z + deltaZ);

    // Interpolation
    public static Vector3Data Lerp(Vector3Data a, Vector3Data b, float t)
    {
      t = FMath.Clamp01(t); // instead of Math.Max/Min
      return new Vector3Data(
          FMath.Lerp(a.X, b.X, t),
          FMath.Lerp(a.Y, b.Y, t),
          FMath.Lerp(a.Z, b.Z, t)
      );
    }

    // Utility
    public static bool IsNearlyZero(
      this Vector3Data v,
      float epsilon = FMath.KINDA_SMALL_NUMBER) =>
      v.SqrMagnitude() <= epsilon * epsilon;

    public static Vector3Data Abs(this Vector3Data v) =>
      new(FMath.Abs(v.X), FMath.Abs(v.Y), FMath.Abs(v.Z));

    public static float MaxComponent(this Vector3Data v) =>
      FMath.Max(stackalloc float[3] { v.X, v.Y, v.Z });

    public static float MinComponent(this Vector3Data v) =>
      FMath.Min(stackalloc float[3] { v.X, v.Y, v.Z });


    // Projection (XZ plane)
    public static float GetLengthXZ(this Vector3Data v) =>
      FMath.FastSqrt(v.X * v.X + v.Z * v.Z);

    public static Vector3Data ProjectOnPlaneXZ(this Vector3Data v) =>
      new(v.X, 0, v.Z);
  }
}
