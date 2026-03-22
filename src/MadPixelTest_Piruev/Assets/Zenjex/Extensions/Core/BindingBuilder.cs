// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System;
using System.Linq;
using System.Reflection;

using Reflex.Core;
using Reflex.Enums;

using U = UnityEngine;

namespace Zenjex.Extensions.Core
{
  /// <summary>
  /// Fluent binding builder — the core of Zenjex's Zenject-like API.
  ///
  /// Obtained via <c>builder.Bind&lt;T&gt;()</c> or <c>builder.BindInstance&lt;T&gt;(instance)</c>.
  ///
  /// Chain modifiers in any order, then terminate with a lifetime method:
  /// <code>
  /// // Simple type binding
  /// builder.Bind&lt;IInputService&gt;()
  ///        .To&lt;InputService&gt;()
  ///        .AsSingle();
  ///
  /// // Prefab-based singleton with hierarchy organisation
  /// builder.Bind&lt;CurtainService&gt;()
  ///        .BindInterfacesAndSelf()
  ///        .FromComponentInNewPrefab(curtainPrefab)
  ///        .WithGameObjectName("Curtain")
  ///        .UnderTransformGroup("Infrastructure")
  ///        .NonLazy();
  ///
  /// // Constructor with mixed container + explicit args
  /// builder.Bind&lt;InputService&gt;()
  ///        .BindInterfacesAndSelf()
  ///        .WithArguments(playerInput, cinemachineInputProvider)
  ///        .AsSingle();
  ///
  /// // Scoped: fresh instance per SceneInstaller sub-container
  /// builder.Bind&lt;LevelProgressServiceResolver&gt;()
  ///        .BindInterfacesAndSelf()
  ///        .CopyIntoDirectSubContainers()
  ///        .NonLazy();
  /// </code>
  /// </summary>
  public sealed class BindingBuilder<T>
  {
    private bool _committed;

    private readonly ContainerBuilder _builder;

    private Type _concrete;
    private Type[] _contracts;
    private Lifetime _lifetime = Lifetime.Transient;
    private Resolution _resolution = Resolution.Lazy;

    // FromInstance
    private object _instance;

    // FromComponentInHierarchy
    private bool _fromHierarchy;
    private bool _includeInactive = true;

    // FromComponentInNewPrefab
    private U.GameObject _prefab;
    private string _gameObjectName;
    private string _transformGroup;
    private U.Transform _parentTransform;

    // WithArguments
    private object[] _extraArgs;

    // CopyIntoDirectSubContainers (Scoped lifetime override)
    private bool _scoped;

    internal BindingBuilder(ContainerBuilder builder)
    {
      _builder = builder;
      _contracts = new[] { typeof(T) };
    }

    #region SOURCE MODIFIERS

    /// <summary>
    /// Bind interface/base-class T to a concrete implementation TConcrete.
    /// Equivalent to Zenject's <c>BindInterfacesAndSelfTo&lt;TConcrete&gt;()</c>.
    /// </summary>
    public BindingBuilder<T> To<TConcrete>() where TConcrete : T
    {
      _concrete = typeof(TConcrete);
      return this;
    }

    /// <summary>
    /// Bind an already-constructed instance.
    /// Equivalent to Zenject's <c>BindInstance(x).AsSingle()</c>.
    /// Always registers as Singleton.
    /// </summary>
    public BindingBuilder<T> FromInstance(T instance)
    {
      _instance = instance;
      _lifetime = Lifetime.Singleton;
      return this;
    }

    /// <summary>
    /// Instantiate a prefab at resolve-time and extract a component of type T
    /// (or of TConcrete if <c>To&lt;TConcrete&gt;()</c> was called).
    ///
    /// Equivalent to Zenject's <c>FromComponentInNewPrefab(prefab)</c>.
    /// Always Singleton + Lazy — prefab is spawned once, on first resolve.
    ///
    /// Chain <see cref="WithGameObjectName"/> and <see cref="UnderTransformGroup"/>
    /// to organise the hierarchy (mirrors Zenject's named modifiers):
    /// <code>
    /// builder.Bind&lt;ICurtainService&gt;()
    ///        .To&lt;CurtainService&gt;()
    ///        .FromComponentInNewPrefab(curtainPrefab)
    ///        .WithGameObjectName("Curtain")
    ///        .UnderTransformGroup("Infrastructure")
    ///        .BindInterfacesAndSelf()
    ///        .NonLazy();
    /// </code>
    /// </summary>
    public BindingBuilder<T> FromComponentInNewPrefab(U.GameObject prefab)
    {
      _prefab = prefab;
      _lifetime = Lifetime.Singleton;
      return this;
    }

    /// <summary>
    /// Renames the root GameObject of the instantiated prefab.
    /// Equivalent to Zenject's <c>.WithGameObjectName("name")</c>.
    /// Only meaningful with <see cref="FromComponentInNewPrefab"/>.
    /// </summary>
    public BindingBuilder<T> WithGameObjectName(string name)
    {
      _gameObjectName = name;
      return this;
    }

    /// <summary>
    /// Places the instantiated prefab under a named group at the scene root.
    /// The group is found by name or created automatically if absent.
    /// Equivalent to Zenject's <c>.UnderTransformGroup("name")</c>.
    /// Only meaningful with <see cref="FromComponentInNewPrefab"/>.
    /// </summary>
    public BindingBuilder<T> UnderTransformGroup(string groupName)
    {
      _transformGroup = groupName;
      _parentTransform = null;
      return this;
    }

    /// <summary>
    /// Places the instantiated prefab under a specific Transform.
    /// Equivalent to Zenject's <c>.UnderTransform(transform)</c>.
    /// Only meaningful with <see cref="FromComponentInNewPrefab"/>.
    /// If both <see cref="UnderTransformGroup"/> and this are called, the last one wins.
    /// </summary>
    public BindingBuilder<T> UnderTransform(U.Transform parent)
    {
      _parentTransform = parent;
      _transformGroup = null;
      return this;
    }

    /// <summary>
    /// Resolves the binding by finding an existing component in the loaded scene hierarchy.
    /// Resolution is always lazy and singleton.
    /// </summary>
    public BindingBuilder<T> FromComponentInHierarchy(bool includeInactive = true)
    {
      _fromHierarchy = true;
      _includeInactive = includeInactive;
      _lifetime = Lifetime.Singleton;
      return this;
    }

    #endregion

    #region ARGUMENT INJECTION

    /// <summary>
    /// Provides explicit constructor arguments that bypass the container.
    ///
    /// Equivalent to Zenject's <c>.WithArguments(arg1, arg2, ...)</c>.
    ///
    /// Zenjex matches each provided value to a constructor parameter by TYPE.
    /// Unmatched parameters are resolved normally from the DI container.
    /// This lets you mix runtime-created objects (e.g. components grabbed after
    /// Instantiate) with container-managed services in the same constructor.
    ///
    /// <code>
    /// var playerInput = Instantiate(playerInputPrefab).GetComponent&lt;PlayerInput&gt;();
    ///
    /// builder.Bind&lt;IInputService&gt;()
    ///        .To&lt;InputService&gt;()
    ///        .WithArguments(playerInput, cinemachineInputProvider)
    ///        .AsSingle();
    ///
    /// // InputService constructor:
    /// // public InputService(PlayerInput pi, CinemachineInputProvider cip, ILoggingService log)
    /// // pi, cip ← WithArguments  |  log ← container
    /// </code>
    /// </summary>
    public BindingBuilder<T> WithArguments(params object[] args)
    {
      _extraArgs = args;
      return this;
    }

    #endregion

    #region CONTRACT MODIFIERS

    /// <summary>
    /// Registers under all interfaces of the concrete type.
    /// Equivalent to Zenject's <c>BindInterfaces()</c>.
    /// </summary>
    public BindingBuilder<T> BindInterfaces()
    {
      var type = _concrete ?? typeof(T);
      _contracts = type.GetInterfaces();
      return this;
    }

    /// <summary>
    /// Registers under all interfaces AND the concrete type itself.
    /// Equivalent to Zenject's <c>BindInterfacesAndSelfTo&lt;T&gt;()</c>.
    /// </summary>
    public BindingBuilder<T> BindInterfacesAndSelf()
    {
      var type = _concrete ?? typeof(T);
      _contracts = type.GetInterfaces().Append(type).ToArray();
      return this;
    }

    #endregion

    #region SUB-CONTAINER SUPPORT

    /// <summary>
    /// Marks this binding to use <b>Scoped</b> lifetime so it is freshly
    /// instantiated inside each <see cref="Scene.SceneInstaller"/> sub-container.
    ///
    /// Equivalent to Zenject's <c>.CopyIntoDirectSubContainers()</c>.
    ///
    /// A scoped instance receives both global services (from RootContainer) and
    /// scene-local bindings (from SceneInstaller) in its constructor, enabling
    /// the Service-Watcher-Resolver pattern from the article:
    ///
    /// <code>
    /// // Global installer:
    /// builder.Bind&lt;LevelProgressServiceResolver&gt;()
    ///        .BindInterfacesAndSelf()
    ///        .CopyIntoDirectSubContainers()
    ///        .NonLazy();
    ///
    /// // SceneInstaller:
    /// builder.BindInstance(levelProgressWatcher).AsSingle();
    /// // LevelProgressServiceResolver gets both ILevelProgressService (global)
    /// // and LevelProgressWatcher (scene-local) from its container.
    /// </code>
    /// </summary>
    public BindingBuilder<T> CopyIntoDirectSubContainers()
    {
      _scoped = true;
      return this;
    }

    #endregion

    #region LIFETIME TERMINATORS

    /// <summary>
    /// Lazy singleton — one shared instance, created on first resolve.
    /// Equivalent to Zenject's <c>.AsSingle()</c>.
    /// </summary>
    public void AsSingle() => Terminate(
      _scoped ? Lifetime.Scoped : Lifetime.Singleton,
      Resolution.Lazy);

    /// <summary>Alias for <see cref="AsSingle"/>.</summary>
    public void AsSingleton() => AsSingle();

    /// <summary>
    /// Eager singleton — created immediately when the container is built.
    ///
    /// In Zenject this is written as <c>.AsSingle().NonLazy()</c> — a two-step chain.
    /// In Zenjex, <c>.NonLazy()</c> is a standalone terminator:
    /// <code>
    /// // Zenject:  .AsSingle().NonLazy()
    /// // Zenjex:   .NonLazy()
    /// </code>
    /// </summary>
    public void NonLazy() => Terminate(
      _scoped ? Lifetime.Scoped : Lifetime.Singleton,
      Resolution.Eager);

    /// <summary>Alias for <see cref="NonLazy"/>.</summary>
    public void AsEagerSingleton() => NonLazy();

    /// <summary>New instance on every resolve.</summary>
    public void AsTransient() => Terminate(Lifetime.Transient, Resolution.Lazy);

    /// <summary>
    /// One instance per sub-container (SceneInstaller).
    /// Equivalent to <c>CopyIntoDirectSubContainers().AsSingle()</c>.
    /// </summary>
    public void AsScoped() => Terminate(Lifetime.Scoped, Resolution.Lazy);

    #endregion

    #region INTERNALS

    private void Terminate(Lifetime lifetime, Resolution resolution)
    {
      if (_scoped && lifetime == Lifetime.Singleton)
        lifetime = Lifetime.Scoped;

      _lifetime = lifetime;
      _resolution = resolution;
      Commit();
    }

    private void Commit()
    {
      if (_committed)
        throw new InvalidOperationException("[Zenjex] Binding already committed.");

      if (_lifetime == Lifetime.Transient && _resolution == Resolution.Eager)
        throw new InvalidOperationException("[Zenjex] Transient + Eager binding is not allowed.");

      _committed = true;

      if (_fromHierarchy) { CommitFromHierarchy(); return; }
      if (_prefab != null) { CommitFromNewPrefab(); return; }
      if (_instance != null) { CommitFromInstance(); return; }
      if (_extraArgs?.Length > 0) { CommitWithArguments(); return; }
      CommitType();
    }

    #region Instance

    private void CommitFromInstance() =>
      _builder.RegisterValue(_instance, EffectiveContracts());

    #endregion

    #region Plain type

    private void CommitType()
    {
      var concrete = _concrete ?? typeof(T);
      _builder.RegisterType(concrete, EffectiveContracts(), _lifetime, _resolution);
    }

    #endregion

    #region WithArguments: mixed container + explicit args

    private void CommitWithArguments()
    {
      var concreteType = _concrete ?? typeof(T);
      var contracts = EffectiveContracts();
      var extraArgs = _extraArgs;

      _builder.RegisterFactory<T>(
        container =>
        {
          var ctor = PickBestConstructor(concreteType);
          var parameters = ctor.GetParameters();
          var args = new object[parameters.Length];

          for (var i = 0; i < parameters.Length; i++)
          {
            var paramType = parameters[i].ParameterType;

            // Match by type from provided extra args (first match wins).
            // This mirrors Zenject's WithArguments behaviour exactly.
            var provided = Array.Find(
              extraArgs,
              a => a != null && paramType.IsAssignableFrom(a.GetType()));

            args[i] = provided ?? container.Resolve(paramType);
          }

          try
          {
            return (T)ctor.Invoke(args);
          }
          catch (Exception ex)
          {
            throw new InvalidOperationException(
              $"[Zenjex] WithArguments: failed to construct '{concreteType.Name}'. " +
              "Verify all unmatched params are registered in the container. " +
              $"Inner: {ex.Message}", ex);
          }
        },
        contracts: contracts,
        lifetime: _lifetime,
        resolution: _resolution
      );
    }

    #endregion

    #region FromComponentInNewPrefab

    private void CommitFromNewPrefab()
    {
      var concreteType = _concrete ?? typeof(T);
      var contracts = EffectiveContracts();
      var prefab = _prefab;
      var goName = _gameObjectName;
      var group = _transformGroup;
      var explicitParent = _parentTransform;

      // Always Singleton + Lazy:
      //   Singleton: one authoritative instance per prefab binding.
      //   Lazy:      scene hierarchy must be initialised before Instantiate() runs;
      //              eager resolution fires during ContainerBuilder.Build() which is
      //              too early (inside ProjectRootInstaller.Awake() at order -280).
      _builder.RegisterFactory<T>(
        factory: _ =>
        {
          var parent = ResolveParentTransform(group, explicitParent);
          var go = U.Object.Instantiate(prefab, parent);

          if (!string.IsNullOrEmpty(goName))
            go.name = goName;

          // FIX CS0413: T may be an interface, so 'as T' is not allowed.
          // Cast via (T)(object) works for both class and interface constraints.
          var component =
              (T)(object)go.GetComponent(concreteType)
           ?? (T)(object)go.GetComponentInChildren(concreteType, includeInactive: true);

          if (component == null)
            throw new InvalidOperationException(
              $"[Zenjex] FromComponentInNewPrefab<{typeof(T).Name}>: " +
              $"prefab '{prefab.name}' has no component of type '{concreteType.Name}'. " +
              "Check the prefab root and its children.");

          return component;
        },
        contracts: contracts,
        lifetime: Lifetime.Singleton,
        resolution: Resolution.Lazy
      );
    }

    #endregion

    #region FromComponentInHierarchy

    private void CommitFromHierarchy()
    {
      var searchType = _concrete ?? typeof(T);
      var contracts = EffectiveContracts();
      var includeInactive = _includeInactive;

      _builder.RegisterFactory<T>(
        factory: _ => FindComponentOrThrow(searchType, includeInactive),
        contracts: contracts,
        lifetime: Lifetime.Singleton,
        resolution: Resolution.Lazy
      );
    }

    #endregion

    #endregion

    #region STATIC HELPERS

    private Type[] EffectiveContracts()
    {
      if (_contracts?.Length > 0)
        return _contracts;

      return new[] { _concrete ?? typeof(T) };
    }

    /// <summary>
    /// Picks the constructor with the most parameters — mirrors Reflex's default
    /// constructor selection strategy (TypeConstructionInfoCache.Generate).
    /// </summary>
    private static ConstructorInfo PickBestConstructor(Type type)
    {
      var ctors = type.GetConstructors(
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

      if (ctors.Length == 0)
        throw new InvalidOperationException(
          $"[Zenjex] WithArguments: '{type.Name}' has no accessible constructors.");

      return ctors.OrderByDescending(c => c.GetParameters().Length).First();
    }

    /// <summary>
    /// Resolves the parent transform for a new prefab instance.
    /// Priority: explicit Transform > named group (find or create) > scene root (null).
    /// </summary>
    private static U.Transform ResolveParentTransform(
      string groupName, U.Transform explicitParent)
    {
      if (explicitParent != null)
        return explicitParent;

      if (string.IsNullOrEmpty(groupName))
        return null;

      var existing = U.GameObject.Find(groupName);
      if (existing != null)
        return existing.transform;

      return new U.GameObject(groupName).transform;
    }

    private static T FindComponentOrThrow(Type concreteType, bool includeInactive)
    {
      if (!concreteType.IsSubclassOf(typeof(U.Component)))
        throw new InvalidOperationException(
          $"[Zenjex] FromComponentInHierarchy<{typeof(T).Name}>: " +
          $"search type '{concreteType.Name}' must be a Component subclass. " +
          "When binding an interface, call .To<ConcreteMonoBehaviour>() before .FromComponentInHierarchy().");

      var findMode = includeInactive
        ? U.FindObjectsInactive.Include
        : U.FindObjectsInactive.Exclude;

      var found = U.Object.FindAnyObjectByType(concreteType, findMode);
      var component = found != null ? (T)(object)found : default;

      if (component == null)
        throw new InvalidOperationException(
          $"[Zenjex] FromComponentInHierarchy<{typeof(T).Name}>: " +
          $"no component of type '{concreteType.Name}' found in scene hierarchy" +
          (includeInactive ? "." : " (inactive excluded)."));

      return component;
    }

    #endregion

  }
}
