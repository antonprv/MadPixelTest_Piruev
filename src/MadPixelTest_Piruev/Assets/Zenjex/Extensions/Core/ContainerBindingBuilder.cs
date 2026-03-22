// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System;
using System.Linq;

using Reflex.Core;

namespace Zenjex.Extensions.Core
{
  public sealed class ContainerBindingBuilder<T>
  {
    private readonly Container _container;
    private Type[] _contracts;
    private object _instance;

    internal ContainerBindingBuilder(Container container)
    {
      _container = container;
      _contracts = new[] { typeof(T) };
    }

    public ContainerBindingBuilder<T> FromInstance(T instance)
    {
      _instance = instance;
      return this;
    }

    public ContainerBindingBuilder<T> BindInterfaces()
    {
      var type = _instance?.GetType() ?? typeof(T);
      _contracts = type.GetInterfaces();
      return this;
    }

    public ContainerBindingBuilder<T> BindInterfacesAndSelf()
    {
      var type = _instance?.GetType() ?? typeof(T);
      _contracts = type.GetInterfaces().Append(type).ToArray();
      return this;
    }

    public void AsSingle()
    {
      if (_instance == null)
        throw new InvalidOperationException(
            "ContainerBindingBuilder: FromInstance() must be called before AsSingle()");

      _container.RegisterValue(_instance, _contracts);
    }
  }
}
