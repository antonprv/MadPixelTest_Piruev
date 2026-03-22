// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System;
using System.Runtime.InteropServices;

using Code.Common.FastMath;

namespace Code.Common.Domain.DataTypes
{
  /// <summary>
  /// Serializable Quaternion without Unity dependency
  /// STRUCT version - compatible with Unity Quaternion for unsafe conversions
  /// </summary>
  [Serializable]
  [StructLayout(LayoutKind.Sequential)]
  public struct QuatData
  {
    public float X;
    public float Y;
    public float Z;
    public float W;

    // Note: Removed parameterless constructor (not allowed for structs)
    // Default value is automatically (0, 0, 0, 0)

    public QuatData(float x, float y, float z, float w)
    {
      X = x;
      Y = y;
      Z = z;
      W = w;
    }

    // Static constant - changed from => to readonly for performance
    public static readonly QuatData Identity = new QuatData(0, 0, 0, 1);

    public override string ToString() => $"({X:F2}, {Y:F2}, {Z:F2}, {W:F2})";

    public override bool Equals(object obj)
    {
      if (obj is QuatData other)
      {
        return FMath.IsNearlyEqual(X, other.X)
            && FMath.IsNearlyEqual(Y, other.Y)
            && FMath.IsNearlyEqual(Z, other.Z)
            && FMath.IsNearlyEqual(W, other.W);
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
        hash = hash * 31 + W.GetHashCode();
        return hash;
      }
    }
  }
}
