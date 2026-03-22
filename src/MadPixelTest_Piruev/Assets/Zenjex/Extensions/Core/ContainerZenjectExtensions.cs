// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Reflex.Core;

namespace Zenjex.Extensions.Core
{
  public static class ContainerZenjectExtensions
  {
    public static ContainerBindingBuilder<T> Bind<T>(this Container container)
    {
      return new ContainerBindingBuilder<T>(container);
    }
  }
}
