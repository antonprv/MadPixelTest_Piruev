// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System;
using System.Reflection;

using Reflex.Caching;
using Reflex.Delegates;

namespace Reflex.Reflectors
{
  internal interface IActivatorFactory
  {
    ObjectActivator GenerateActivator(Type type, ConstructorInfo constructor, MemberParamInfo[] parameters);
    ObjectActivator GenerateDefaultActivator(Type type);
  }
}