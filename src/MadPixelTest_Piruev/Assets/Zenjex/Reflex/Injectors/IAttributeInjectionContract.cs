// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Reflex.Core;

namespace Reflex.Injectors
{
  public interface IAttributeInjectionContract
  {
    /// <summary>
    /// Automatically invoked by Reflex for dependency injection
    /// </summary>
    void ReflexInject(Container container);
  }
}