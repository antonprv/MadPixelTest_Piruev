// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System;
using System.Runtime.InteropServices;

using Code.Common.FastMath;

namespace Code.Common.Domain.DataTypes
{
  /// <summary>
  /// Serializable Vector3 without Unity dependency
  /// STRUCT version - compatible with Unity Vector3 for unsafe conversions
  /// Can be used in any .NET environment
  /// </summary>
  [Serializable]
  [StructLayout(LayoutKind.Sequential)]
  public struct Vector3Data
  {
    public float X;
    public float Y;
    public float Z;

    // Note: Removed parameterless constructor (not allowed for structs)
    // Default value is automatically (0, 0, 0)

    public Vector3Data(float x, float y, float z)
    {
      X = x;
      Y = y;
      Z = z;
    }

    // Static constants - changed from => to readonly for performance
    // (struct properties with => create new instance on each access)
    public static readonly Vector3Data Zero = new Vector3Data(0, 0, 0);
    public static readonly Vector3Data One = new Vector3Data(1, 1, 1);
    public static readonly Vector3Data Up = new Vector3Data(0, 1, 0);
    public static readonly Vector3Data Down = new Vector3Data(0, -1, 0);
    public static readonly Vector3Data Forward = new Vector3Data(0, 0, 1);
    public static readonly Vector3Data Back = new Vector3Data(0, 0, -1);
    public static readonly Vector3Data Right = new Vector3Data(1, 0, 0);
    public static readonly Vector3Data Left = new Vector3Data(-1, 0, 0);

    public override string ToString() => $"({X:F2}, {Y:F2}, {Z:F2})";

    public override bool Equals(object obj)
    {
      if (obj is Vector3Data other)
      {
        return FMath.IsNearlyEqual(X, other.X)
            && FMath.IsNearlyEqual(Y, other.Y)
            && FMath.IsNearlyEqual(Z, other.Z);
      }
      return false;
    }

    public override int GetHashCode()
    {
      unchecked
      {
        int hash = 17;
        hash = hash * 31 + X.GetHashCode();
        hash = hash * 31 + Y.GetHashCode();
        hash = hash * 31 + Z.GetHashCode();
        return hash;
      }
    }

    // Operator overloads for convenience (optional but recommended for structs)
    public static Vector3Data operator +(Vector3Data a, Vector3Data b) =>
      new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    public static Vector3Data operator -(Vector3Data a, Vector3Data b) =>
      new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    public static Vector3Data operator *(Vector3Data v, float scalar) =>
      new(v.X * scalar, v.Y * scalar, v.Z * scalar);

    public static Vector3Data operator *(float scalar, Vector3Data v) =>
      new(v.X * scalar, v.Y * scalar, v.Z * scalar);

    public static Vector3Data operator /(Vector3Data v, float scalar) =>
      new(v.X / scalar, v.Y / scalar, v.Z / scalar);

    public static Vector3Data operator -(Vector3Data v) =>
      new(-v.X, -v.Y, -v.Z);
  }
}
