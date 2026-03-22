// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

namespace Code.Common.Domain.DataTypes
{
  /// <summary>
  /// Serializable Transform without Unity dependency
  /// Contains Position, Rotation, and Scale
  /// NOTE: TransformData remains a CLASS (container for structs)
  /// </summary>
  [System.Serializable]
  public sealed class TransformData
  {
    public Vector3Data Position;  // Now struct
    public QuatData Rotation;     // Now struct
    public Vector3Data Scale;     // Now struct

    public TransformData()
    {
      Position = Vector3Data.Zero;
      Rotation = QuatData.Identity;
      Scale = Vector3Data.One;
    }

    public TransformData(Vector3Data position, QuatData rotation, Vector3Data scale)
    {
      Position = position;
      Rotation = rotation;
      Scale = scale;
    }

    public static TransformData Identity() =>
      new(Vector3Data.Zero, QuatData.Identity, Vector3Data.One);

    public override string ToString() =>
      $"Pos: {Position}, Rot: {Rotation}, Scale: {Scale}";
  }
}
