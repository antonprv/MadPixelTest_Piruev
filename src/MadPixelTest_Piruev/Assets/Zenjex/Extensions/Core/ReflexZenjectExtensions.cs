// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Reflex.Core;

namespace Zenjex.Extensions.Core
{
  /// <summary>
  /// Extension methods that add Zenject-like <c>Bind</c> and <c>BindInstance</c>
  /// entry-points to <see cref="ContainerBuilder"/>.
  /// </summary>
  public static class ReflexZenjectExtensions
  {
    /// <summary>
    /// Begins a fluent binding for type T.
    /// Returns a <see cref="BindingBuilder{T}"/> — chain modifiers then call a
    /// lifetime terminator (<c>AsSingle</c>, <c>NonLazy</c>, <c>AsTransient</c>, …).
    ///
    /// <code>
    /// builder.Bind&lt;IInputService&gt;()
    ///        .To&lt;InputService&gt;()
    ///        .AsSingle();
    /// </code>
    /// </summary>
    public static BindingBuilder<T> Bind<T>(this ContainerBuilder builder) =>
      new(builder);

    /// <summary>
    /// Shorthand for <c>Bind&lt;T&gt;().FromInstance(instance).AsSingle()</c>.
    /// Equivalent to Zenject's <c>Container.BindInstance(x)</c>.
    ///
    /// Registers a pre-existing instance as a lazy singleton under its own type.
    /// Chain <c>BindInterfaces()</c> / <c>BindInterfacesAndSelf()</c> before the
    /// terminator to also expose it under its interfaces:
    ///
    /// <code>
    /// // Simple — resolves as ConcreteService only:
    /// builder.BindInstance(myService).AsSingle();
    ///
    /// // Full — also resolves as IMyService, IDisposable, …:
    /// builder.BindInstance(myService)
    ///        .BindInterfacesAndSelf()
    ///        .AsSingle();
    /// </code>
    /// </summary>
    public static BindingBuilder<T> BindInstance<T>(this ContainerBuilder builder, T instance) =>
      new BindingBuilder<T>(builder).FromInstance(instance);
  }
}
