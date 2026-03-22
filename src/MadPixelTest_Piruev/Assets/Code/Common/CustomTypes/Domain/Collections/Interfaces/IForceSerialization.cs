// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

namespace Code.Common.CustomTypes.Domain.Collections.Interfaces
{
  /// <summary>
  /// Interface for types that support manual serialization triggering.
  /// </summary>
  public interface IForceSerialization
  {
    /// <summary>
    /// Forces synchronization of data to serialized format.
    /// </summary>
    void ForceSerialization();
  }
}
