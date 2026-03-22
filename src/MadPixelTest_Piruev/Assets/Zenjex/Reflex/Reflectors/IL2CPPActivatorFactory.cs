// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System;
using System.Reflection;

using Reflex.Caching;
using Reflex.Delegates;

namespace Reflex.Reflectors
{
  internal sealed class IL2CPPActivatorFactory : IActivatorFactory
  {
    public ObjectActivator GenerateActivator(Type type, ConstructorInfo constructor, MemberParamInfo[] parameters)
    {
      return args =>
      {
        var instance = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(type);
        constructor.Invoke(instance, args);
        return instance;
      };
    }

    public ObjectActivator GenerateDefaultActivator(Type type)
    {
      return args => System.Runtime.Serialization.FormatterServices.GetUninitializedObject(type);
    }
  }
}