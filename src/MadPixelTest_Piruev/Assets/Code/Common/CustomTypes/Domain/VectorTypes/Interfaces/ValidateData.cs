// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

namespace Code.Common.CustomTypes.Domain.VectorTypes.Interfaces
{
  public static class ValidateData
  {
    public static bool IsValid<TData>(this TData data) where TData : class, IValidatableData =>
      data.IsValid();
  }
}
