// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Common.Domain.DataTypes;
using Code.Common.FastMath;


namespace Code.Common.Domain.VectorTypes.Extensions
{
  public static partial class QuatDataExtensions
  {
    /// <summary>
    /// Converts 2D input to a world direction on the XZ-plane
    /// Optimized for frequent calls
    /// </summary>
    public static Vector3Data ScreenInputToWorldDirection(
      Vector3Data cameraForward,
      Vector3Data cameraRight,
      Vector2Data inputAxis,
      float minInputThreshold = FMath.KINDA_SMALL_NUMBER)
    {
      // Components directly on the stack
      float x = cameraForward.X * inputAxis.Y + cameraRight.X * inputAxis.X;
      float y = cameraForward.Y * inputAxis.Y + cameraRight.Y * inputAxis.X;
      float z = cameraForward.Z * inputAxis.Y + cameraRight.Z * inputAxis.X;

      // Projection onto XZ plane
      y = 0f;

      float sqrMag = x * x + y * y + z * z;
      if (sqrMag < minInputThreshold * minInputThreshold)
        return Vector3Data.Zero;

      float invMag = FMath.FastInvSqrt(sqrMag);
      return new Vector3Data(x * invMag, y * invMag, z * invMag);
    }

    /// <summary>
    /// LookRotation using FMath without extra allocations
    /// </summary>
    public static QuatData LookRotation(Vector3Data forward, Vector3Data up)
    {
      float fx = forward.X;
      float fy = forward.Y;
      float fz = forward.Z;

      float ux = up.X;
      float uy = up.Y;
      float uz = up.Z;

      // Normalize forward vector
      float fSqr = fx * fx + fy * fy + fz * fz;
      if (fSqr < FMath.KINDA_SMALL_NUMBER)
        return QuatData.Identity;

      float fInvMag = FMath.FastInvSqrt(fSqr);
      fx *= fInvMag;
      fy *= fInvMag;
      fz *= fInvMag;

      // Right vector r = forward × up
      float rx = fy * uz - fz * uy;
      float ry = fz * ux - fx * uz;
      float rz = fx * uy - fy * ux;

      float rSqr = rx * rx + ry * ry + rz * rz;
      if (rSqr < FMath.KINDA_SMALL_NUMBER)
      {
        // Forward and up vectors are nearly collinear, just take Identity
        return QuatData.Identity;
      }

      float rInvMag = FMath.FastInvSqrt(rSqr);
      rx *= rInvMag;
      ry *= rInvMag;
      rz *= rInvMag;

      // Recalculate up = r × forward
      float ux2 = ry * fz - rz * fy;
      float uy2 = rz * fx - rx * fz;
      float uz2 = rx * fy - ry * fx;

      // Construct quaternion from matrix (columns: r, up, -forward)
      float m00 = rx, m01 = ux2, m02 = -fx;
      float m10 = ry, m11 = uy2, m12 = -fy;
      float m20 = rz, m21 = uz2, m22 = -fz;

      float trace = m00 + m11 + m22;
      float qw, qx, qy, qz;

      if (trace > 0f)
      {
        float s = FMath.FastSqrt(trace + 1f) * 2f;
        float invS = 1f / s;
        qw = 0.25f * s;
        qx = (m21 - m12) * invS;
        qy = (m02 - m20) * invS;
        qz = (m10 - m01) * invS;
      }
      else if (m00 > m11 && m00 > m22)
      {
        float s = FMath.FastSqrt(1f + m00 - m11 - m22) * 2f;
        float invS = 1f / s;
        qw = (m21 - m12) * invS;
        qx = 0.25f * s;
        qy = (m01 + m10) * invS;
        qz = (m02 + m20) * invS;
      }
      else if (m11 > m22)
      {
        float s = FMath.FastSqrt(1f + m11 - m00 - m22) * 2f;
        float invS = 1f / s;
        qw = (m02 - m20) * invS;
        qx = (m01 + m10) * invS;
        qy = 0.25f * s;
        qz = (m12 + m21) * invS;
      }
      else
      {
        float s = FMath.FastSqrt(1f + m22 - m00 - m11) * 2f;
        float invS = 1f / s;
        qw = (m10 - m01) * invS;
        qx = (m02 + m20) * invS;
        qy = (m12 + m21) * invS;
        qz = 0.25f * s;
      }

      // Normalize quaternion using FastInvSqrt
      float qSqr = qx * qx + qy * qy + qz * qz + qw * qw;
      float qInv = FMath.FastInvSqrt(qSqr);

      return new QuatData(qx * qInv, qy * qInv, qz * qInv, qw * qInv);
    }

    /// <summary>
    /// LookRotation with up = Vector3Data.Up
    /// </summary>
    public static QuatData LookRotation(Vector3Data forward) =>
        LookRotation(forward, Vector3Data.Up);
  }
}
