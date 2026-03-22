// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System.Runtime.CompilerServices;

using Unity.Burst;
using Unity.Mathematics;

namespace Code.Common.FastMath.Infrastructure
{
  [BurstCompile]
  public static class BurstMath
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float BurstInvSqrt(float x) => math.rsqrt(x);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float BurstSqrt(float x) => math.sqrt(x);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float BurstDistanceSquared(float3 a, float3 b) => math.dot(b - a, b - a);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float BurstDistance(float3 a, float3 b) => math.distance(a, b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 BurstNormalize(float3 v) => math.normalize(v);
  }
}
