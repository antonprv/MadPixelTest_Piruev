// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System;
using System.Runtime.CompilerServices;

using Code.Common.CustomTypes.Infrastructure.Types;

using Code.Common.Domain.DataTypes;

using UnityEngine;

namespace Code.External.Infrastructure.Unity
{
  /// <summary>
  /// High-performance conversion extensions using unsafe code
  /// Designed for scenarios with hundreds of thousands of calls per frame
  /// </summary>
  public static class UnityConversionExtensions
  {
    #region Vector3 Conversions

    /// <summary>
    /// Convert Unity Vector3 to domain Vector3Data (zero-copy when memory layout matches)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Vector3Data ToVector3Data(this Vector3 unityVector) =>
      // Direct memory reinterpretation - assumes Vector3Data has same layout as Vector3
      *(Vector3Data*)&unityVector;

    /// <summary>
    /// Convert domain Vector3Data to Unity Vector3 (zero-copy when memory layout matches)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Vector3 ToUnityVector3(this Vector3Data dataVector) =>
      // Direct memory reinterpretation
      *(Vector3*)&dataVector;

    /// <summary>
    /// Batch convert Unity Vector3 array to Vector3Data array
    /// </summary>
    public static unsafe void ToVector3DataBatch(ReadOnlySpan<Vector3> source, Span<Vector3Data> destination)
    {
      if (source.Length != destination.Length)
        throw new ArgumentException("Source and destination spans must have the same length");

      fixed (Vector3* srcPtr = source)
      fixed (Vector3Data* dstPtr = destination)
      {
        Buffer.MemoryCopy(srcPtr, dstPtr,
          destination.Length * sizeof(Vector3Data),
          source.Length * sizeof(Vector3));
      }
    }

    /// <summary>
    /// Batch convert Vector3Data array to Unity Vector3 array
    /// </summary>
    public static unsafe void ToUnityVector3Batch(ReadOnlySpan<Vector3Data> source, Span<Vector3> destination)
    {
      if (source.Length != destination.Length)
        throw new ArgumentException("Source and destination spans must have the same length");

      fixed (Vector3Data* srcPtr = source)
      fixed (Vector3* dstPtr = destination)
      {
        Buffer.MemoryCopy(srcPtr, dstPtr,
          destination.Length * sizeof(Vector3),
          source.Length * sizeof(Vector3Data));
      }
    }

    #endregion

    #region Quaternion Conversions

    /// <summary>
    /// Convert Unity Quaternion to domain QuatData (zero-copy)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe QuatData ToQuatData(this Quaternion unityQuat) =>
      *(QuatData*)&unityQuat;

    /// <summary>
    /// Convert domain QuatData to Unity Quaternion (zero-copy)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Quaternion ToUnityQuaternion(this QuatData dataQuat) =>
      *(Quaternion*)&dataQuat;

    /// <summary>
    /// Batch convert Unity Quaternion array to QuatData array
    /// </summary>
    public static unsafe void ToQuaternionDataBatch(ReadOnlySpan<Quaternion> source, Span<QuatData> destination)
    {
      if (source.Length != destination.Length)
        throw new ArgumentException("Source and destination spans must have the same length");

      fixed (Quaternion* srcPtr = source)
      fixed (QuatData* dstPtr = destination)
      {
        Buffer.MemoryCopy(srcPtr, dstPtr,
          destination.Length * sizeof(QuatData),
          source.Length * sizeof(Quaternion));
      }
    }

    #endregion

    #region Transform Conversions

    /// <summary>
    /// Convert Unity Transform to domain TransformData
    /// Note: This still allocates TransformData, but minimizes internal allocations
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TransformData ToTransformData(this Transform transform) =>
      new TransformData(
        transform.position.ToVector3Data(),
        transform.rotation.ToQuatData(),
        transform.localScale.ToVector3Data()
      );

    /// <summary>
    /// Convert Unity Transform to domain TransformData using out parameter (avoids return copy)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ToTransformData(this Transform transform, out TransformData result) =>
      result = new TransformData(
        transform.position.ToVector3Data(),
        transform.rotation.ToQuatData(),
        transform.localScale.ToVector3Data()
      );

    /// <summary>
    /// Apply domain TransformData to Unity Transform (optimized)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ApplyCoordinates(this Transform transform, TransformData data)
    {
      if (data == null) return;

      transform.SetPositionAndRotation(
        data.Position.ToUnityVector3(),
        data.Rotation.ToUnityQuaternion()
      );
      transform.localScale = data.Scale.ToUnityVector3();
    }

    /// <summary>
    /// Apply domain TransformData to Unity Transform using ref (avoids defensive copy)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ApplyTransformData(this Transform transform, in TransformData data)
    {
      if (data == null) return;

      transform.SetPositionAndRotation(
        data.Position.ToUnityVector3(),
        data.Rotation.ToUnityQuaternion()
      );
      transform.localScale = data.Scale.ToUnityVector3();
    }

    /// <summary>
    /// Batch apply TransformData to multiple transforms
    /// </summary>
    public static void ApplyTransformDataBatch(Transform[] transforms, ReadOnlySpan<TransformData> dataArray)
    {
      int count = Math.Min(transforms.Length, dataArray.Length);

      for (int i = 0; i < count; i++)
      {
        if (dataArray[i] != null)
        {
          transforms[i].ApplyTransformData(in dataArray[i]);
        }
      }
    }

    #endregion

    #region Individual Component Setters

    /// <summary>
    /// Set position from Vector3Data (inlined)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetPosition(this Transform transform, Vector3Data position) =>
      transform.position = position.ToUnityVector3();

    /// <summary>
    /// Set rotation from QuatData (inlined)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetRotation(this Transform transform, QuatData rotation) =>
      transform.rotation = rotation.ToUnityQuaternion();

    /// <summary>
    /// Set local scale from Vector3Data (inlined)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetScale(this Transform transform, Vector3Data scale) =>
      transform.localScale = scale.ToUnityVector3();

    /// <summary>
    /// Get position as Vector3Data (inlined)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Data GetPositionData(this Transform transform) =>
      transform.position.ToVector3Data();

    /// <summary>
    /// Get rotation as QuatData (inlined)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static QuatData GetRotationData(this Transform transform) =>
      transform.rotation.ToQuatData();

    /// <summary>
    /// Get local scale as Vector3Data (inlined)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Data GetScaleData(this Transform transform) =>
      transform.localScale.ToVector3Data();

    #endregion

    #region Coordinate System

    /// <summary>
    /// Create Coordinates struct from TransformData (inlined)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Coordinates ToCoordinates(this TransformData data) =>
      new Coordinates(
        data.Position.ToUnityVector3(),
        data.Rotation.ToUnityQuaternion()
      );

    /// <summary>
    /// Create Coordinates struct from Transform (inlined)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Coordinates ToCoordinates(this Transform transform) =>
      new Coordinates(
        transform.position,
        transform.rotation
      );

    /// <summary>
    /// Convert Unity Transform to domain TransformData
    /// Note: This still allocates TransformData, but minimizes internal allocations
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TransformData ToTransformData(
      this Coordinates coords, Vector3 localScale) => new(
        coords.Position.ToVector3Data(),
        coords.Rotation.ToQuatData(),
        localScale.ToVector3Data()
      );

    /// <summary>
    /// Convert Unity Transform to domain TransformData using out parameter (avoids return copy)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ToTransformData(
      this Coordinates coords, Vector3 localScale, out TransformData result) =>
      result = new TransformData(
        coords.Position.ToVector3Data(),
        coords.Rotation.ToQuatData(),
        localScale.ToVector3Data()
      );


    /// <summary>
    /// Apply domain TransformData to Unity Transform (optimized)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ApplyCoordinates(this Transform transform, Coordinates coords)
    {
      if (coords == null) return;

      transform.SetPositionAndRotation(
        coords.Position,
        coords.Rotation
      );
    }

    #endregion

    #region Pooled Conversions (For extreme performance scenarios)

    /// <summary>
    /// Thread-local pooled array for batch conversions
    /// Use this for temporary batch operations to avoid allocations
    /// </summary>
    [ThreadStatic]
    private static Vector3Data[] _vector3DataPool;

    [ThreadStatic]
    private static Vector3[] _vector3Pool;

    private const int DefaultPoolSize = 1024;

    /// <summary>
    /// Get pooled Vector3Data array (thread-safe via ThreadStatic)
    /// </summary>
    public static Vector3Data[] GetPooledVector3DataArray(int minSize)
    {
      if (_vector3DataPool == null || _vector3DataPool.Length < minSize)
        _vector3DataPool = new Vector3Data[Math.Max(minSize, DefaultPoolSize)];

      return _vector3DataPool;
    }

    /// <summary>
    /// Get pooled Vector3 array (thread-safe via ThreadStatic)
    /// </summary>
    public static Vector3[] GetPooledVector3Array(int minSize)
    {
      if (_vector3Pool == null || _vector3Pool.Length < minSize)
        _vector3Pool = new Vector3[Math.Max(minSize, DefaultPoolSize)];

      return _vector3Pool;
    }

    #endregion
  }
}
