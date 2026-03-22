// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System;
using System.Collections.Generic;

namespace Reflex.Resolvers
{
  public sealed class ResolverDebugProperties
  {
    public int Resolutions;
    public List<(WeakReference, List<CallSite>)> Instances { get; } = new();
    public List<CallSite> BindingCallsite { get; } = new();
  }
}