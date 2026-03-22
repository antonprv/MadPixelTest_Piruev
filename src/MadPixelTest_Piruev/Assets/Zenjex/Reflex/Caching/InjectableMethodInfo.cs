// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System.Reflection;

namespace Reflex.Caching
{
  internal sealed class InjectableMethodInfo
  {
    public readonly MethodInfo MethodInfo;
    public readonly ParameterInfo[] Parameters;

    public InjectableMethodInfo(MethodInfo methodInfo)
    {
      MethodInfo = methodInfo;
      Parameters = methodInfo.GetParameters();
    }
  }
}