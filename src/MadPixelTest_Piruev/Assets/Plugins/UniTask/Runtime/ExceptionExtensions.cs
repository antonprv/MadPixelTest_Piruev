// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System;

namespace Cysharp.Threading.Tasks
{
  public static class ExceptionExtensions
  {
    public static bool IsOperationCanceledException(this Exception exception)
    {
      return exception is OperationCanceledException;
    }
  }
}

