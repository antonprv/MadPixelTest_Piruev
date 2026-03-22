// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System.Reflection;

namespace Reflex.Caching
{
  internal sealed class TypeAttributeInfo
  {
    public readonly FieldInfo[] InjectableFields;
    public readonly PropertyInfo[] InjectableProperties;
    public readonly InjectableMethodInfo[] InjectableMethods;

    public TypeAttributeInfo(FieldInfo[] injectableFields, PropertyInfo[] injectableProperties, InjectableMethodInfo[] injectableMethods)
    {
      InjectableFields = injectableFields;
      InjectableProperties = injectableProperties;
      InjectableMethods = injectableMethods;
    }
  }
}