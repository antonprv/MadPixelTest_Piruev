// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Common.FastMath;

using Unity.Burst;
using Unity.Mathematics;

using UnityEngine;

[BurstCompile]
public static class UnityMathExtensions
{
  [BurstCompile]
  public static bool IsNearlyEqualBurst(
      ref quaternion quat, ref quaternion other,
      float epsilon = FMath.KINDA_SMALL_NUMBER)
  {
    return math.abs(quat.value.x - other.value.x) <= epsilon
        && math.abs(quat.value.y - other.value.y) <= epsilon
        && math.abs(quat.value.z - other.value.z) <= epsilon
        && math.abs(quat.value.w - other.value.w) <= epsilon;
  }

  public static bool IsNearlyEqual(
    this Quaternion quat, Quaternion other,
      float epsilon = FMath.KINDA_SMALL_NUMBER)
  {
    return FMath.Abs(quat.x - other.x) <= epsilon
        && FMath.Abs(quat.y - other.y) <= epsilon
        && FMath.Abs(quat.z - other.z) <= epsilon
        && FMath.Abs(quat.w - other.w) <= epsilon;
  }

  public static float GetLengthXZ(this Vector3 vec) =>
    FMath.FastLength(vec.x, 0f, vec.z);
}
