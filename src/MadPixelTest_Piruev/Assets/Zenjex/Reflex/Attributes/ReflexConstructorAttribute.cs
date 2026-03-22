// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System;

using JetBrains.Annotations;

namespace Reflex.Attributes
{
  [MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
  [AttributeUsage(AttributeTargets.Constructor)]
  public class ReflexConstructorAttribute : Attribute
  {
  }
}