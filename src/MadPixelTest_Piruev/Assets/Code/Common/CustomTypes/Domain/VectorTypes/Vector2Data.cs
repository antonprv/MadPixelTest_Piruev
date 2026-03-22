// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

namespace Code.Common.Domain.VectorTypes
{
  [System.Serializable]
  public struct Vector2Data
  {
    public float X;
    public float Y;

    public Vector2Data(float x, float y)
    {
      X = x;
      Y = y;
    }

    public readonly float SqrMagnitude => (X * X) + Y * Y;
    public static Vector2Data Zero => new(0, 0);
  }
}
