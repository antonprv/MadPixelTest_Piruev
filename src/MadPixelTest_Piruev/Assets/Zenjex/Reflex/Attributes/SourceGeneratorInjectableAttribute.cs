// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System;

using JetBrains.Annotations;

namespace Reflex.Attributes
{
  [MeansImplicitUse(ImplicitUseKindFlags.Assign)]
  [AttributeUsage(AttributeTargets.Class, Inherited = true)]
  public class SourceGeneratorInjectableAttribute : Attribute
  {
  }
}