// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using UnityEngine;

namespace Code.Common.CustomTypes.Infrastructure.Types
{
  /// <summary>
  /// Unity-specific coordinate system
  /// Uses Unity types directly for performance in gameplay code
  /// </summary>
  [System.Serializable]
  public class Coordinates
  {
    public Vector3 Position;
    public Quaternion Rotation;

    public Coordinates(Vector3 position, Quaternion rotation)
    {
      Position = position;
      Rotation = rotation;
    }

    public static Coordinates Identity() =>
      new Coordinates(Vector3.zero, Quaternion.identity);

    public override string ToString() =>
      $"Pos: {Position}, Rot: {Rotation}";
  }
}
