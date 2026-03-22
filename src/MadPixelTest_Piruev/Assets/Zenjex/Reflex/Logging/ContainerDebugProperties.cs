// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

namespace Reflex.Logging
{
  public sealed class ContainerDebugProperties
  {
    public List<CallSite> BuildCallsite { get; } = new();
  }
}