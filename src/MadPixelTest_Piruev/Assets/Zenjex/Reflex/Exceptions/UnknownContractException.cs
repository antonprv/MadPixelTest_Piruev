// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System;

using Reflex.Extensions;

namespace Reflex.Exceptions
{
  public sealed class UnknownContractException : Exception
  {
    public Type UnknownContract { get; }

    public UnknownContractException(Type unknownContract) : base(GenerateMessage(unknownContract))
    {
      UnknownContract = unknownContract;
    }

    private static string GenerateMessage(Type unknownContract)
    {
      return $"Cannot resolve contract '{unknownContract.GetFullName()}'.";
    }
  }
}