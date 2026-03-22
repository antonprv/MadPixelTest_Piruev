// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

using Zenjex.Extensions.Core;
using Zenjex.Extensions.Injector;

namespace Zenjex.Extensions.Runner
{
  /// <summary>
  /// Injects [Zenjex] members across all scenes in two passes, both driven by
  /// ProjectRootInstaller - the only place where we know the container state for certain.
  ///
  /// Pass 1 - OnContainerReady (fires synchronously inside ProjectRootInstaller.Awake()):
  ///   RootContainer is freshly built. We walk all loaded scenes and inject everything
  ///   resolvable right now. Because this runs inside Awake() at ExecutionOrder -280,
  ///   all subsequent Awake() calls in the scene will already see injected fields.
  ///
  /// Pass 2 - OnGameLaunched (fires after InstallGameInstanceRoutine() completes):
  ///   If InstallGameInstanceRoutine() registered additional bindings, objects not yet
  ///   injected get a second attempt.
  ///
  /// Pass 3 - SceneManager.sceneLoaded:
  ///   Covers scenes loaded additively after launch. Runs after their Awake(),
  ///   so emits ZNX-LATE warnings for plain MonoBehaviour classes.
  ///   Use ZenjexBehaviour for guaranteed pre-Awake injection in additive scenes.
  ///
  /// Runtime - InjectGameObject():
  ///   Call manually right after Instantiate() for dynamically created objects.
  /// </summary>
  public static class ZenjexRunner
  {

    #region Internal state

    private static readonly HashSet<EntityId> _injected = new();
    private static bool _launched;

    #endregion

    #region Debug registry (read by ZenjexDebuggerWindow)

    /// <summary>Records of all injected objects, kept for the debugger window.</summary>
    public static readonly List<InjectedRecord> InjectedRecords = new();

    /// <summary>Fires whenever a new object is injected or the state is reset.</summary>
    public static event Action OnStateChanged;

    #endregion

    internal static bool IsLaunched => _launched;
    public static bool IsReady => _launched;

    #region Bootstrap

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
      _injected.Clear();
      InjectedRecords.Clear();
      _launched = false;

      ProjectRootInstaller.OnContainerReady += OnContainerReady;
      ProjectRootInstaller.OnGameLaunched += OnGameLaunched;
      SceneManager.sceneLoaded += OnSceneLoaded;
    }

    #endregion

    #region Pass 1: inside ProjectRootInstaller.Awake()

    private static void OnContainerReady()
    {
      for (var i = 0; i < SceneManager.sceneCount; i++)
        InjectScene(SceneManager.GetSceneAt(i), pass: InjectionPass.ContainerReady);

      OnStateChanged?.Invoke();
    }

    #endregion

    #region Pass 2: after InstallGameInstanceRoutine()

    private static void OnGameLaunched()
    {
      _launched = true;

      for (var i = 0; i < SceneManager.sceneCount; i++)
        InjectScene(SceneManager.GetSceneAt(i), pass: InjectionPass.GameLaunched);

      OnStateChanged?.Invoke();
    }

    #endregion

    #region Pass 3: additive scenes loaded after launch

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
      if (!_launched) return;
      InjectScene(scene, pass: InjectionPass.SceneLoaded);
      OnStateChanged?.Invoke();
    }

    #endregion

    #region Scene walking

    private static void InjectScene(Scene scene, InjectionPass pass)
    {
      foreach (var root in scene.GetRootGameObjects())
        foreach (var mb in root.GetComponentsInChildren<MonoBehaviour>(includeInactive: true))
          TryInject(mb, pass, scene.name);
    }

    #endregion

    #region Per-object injection

    private static void TryInject(MonoBehaviour mb, InjectionPass pass, string sceneName)
    {
      var id = mb.GetEntityId();
      if (_injected.Contains(id)) return;
      if (!ZenjexInjector.HasZenjexMembers(mb.GetType())) return;

      if (pass == InjectionPass.SceneLoaded)
      {
        Debug.LogWarning(
          $"[Zenjex] ZNX-LATE: '{mb.GetType().Name}' on '{mb.gameObject.name}' was injected " +
          $"after its Awake() ran. Any [Zenjex] fields accessed in Awake() were null. " +
          $"Use ZenjexBehaviour for guaranteed pre-Awake injection.");
      }

      ZenjexInjector.Inject(mb);
      _injected.Add(id);

      InjectedRecords.Add(new InjectedRecord(
        typeName: mb.GetType().Name,
        goName: mb.gameObject.name,
        sceneName: sceneName,
        pass: pass,
        isLate: pass == InjectionPass.SceneLoaded
      ));
    }

    #endregion

    #region Runtime Instantiate support

    /// <summary>
    /// Call immediately after Instantiate() to inject [Zenjex] members
    /// on all MonoBehaviours in the new GameObject hierarchy.
    ///
    ///   var enemy = Instantiate(prefab);
    ///   ZenjexRunner.InjectGameObject(enemy);
    /// </summary>
    public static void InjectGameObject(GameObject go)
    {
      if (!_launched)
      {
        Debug.LogWarning(
          $"[Zenjex] InjectGameObject() called before ProjectRootInstaller.LaunchGame(). " +
          $"'{go.name}' will not be injected.");
        return;
      }

      var sceneName = go.scene.name;
      foreach (var mb in go.GetComponentsInChildren<MonoBehaviour>(includeInactive: true))
        TryInject(mb, InjectionPass.Manual, sceneName);

      OnStateChanged?.Invoke();
    }

    #endregion

    #region Called by ZenjexBehaviour to avoid double injection

    /// <summary>
    /// Registers an instance as already injected so ZenjexRunner won't re-inject it.
    /// Called automatically by ZenjexBehaviour.Awake().
    /// </summary>
    public static void MarkInjected(MonoBehaviour mb)
    {
      var id = mb.GetEntityId();
      if (_injected.Contains(id)) return;

      _injected.Add(id);
      InjectedRecords.Add(new InjectedRecord(
        typeName: mb.GetType().Name,
        goName: mb.gameObject.name,
        sceneName: mb.gameObject.scene.name,
        pass: InjectionPass.ZenjexBehaviour,
        isLate: false
      ));

      OnStateChanged?.Invoke();
    }

    #endregion

    #region Data types

    public enum InjectionPass
    {
      ContainerReady,   // Pass 1 - inside ProjectRootInstaller.Awake()
      GameLaunched,     // Pass 2 - after InstallGameInstanceRoutine()
      SceneLoaded,      // Pass 3 - additive scene loaded after launch (late)
      Manual,           // InjectGameObject() call
      ZenjexBehaviour,  // ZenjexBehaviour.Awake()
    }

    public class InjectedRecord
    {
      public readonly string TypeName;
      public readonly string GoName;
      public readonly string SceneName;
      public readonly InjectionPass Pass;
      public readonly bool IsLate;

      public InjectedRecord(string typeName, string goName, string sceneName, InjectionPass pass, bool isLate)
      {
        TypeName = typeName;
        GoName = goName;
        SceneName = sceneName;
        Pass = pass;
        IsLate = isLate;
      }
    }

    #endregion

  }
}
