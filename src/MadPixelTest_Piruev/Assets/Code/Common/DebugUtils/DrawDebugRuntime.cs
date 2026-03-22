// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Code.Common.DebugUtils
{
  public struct DebugShapeName
  {
    public const string Sphere = "DebugWireSphere";
    public const string Cube = "DebugWireCube";

    public const string SphereTemp = "DebugWireSphereTemp";
    public const string CubeTemp = "DebugWireCubeTemp";
  }

  public static class DrawDebugRuntime
  {
    #region Constants

    private const int DefaultSphereSegments = 24;
    private const string ParentSuffix = "_Parent";
    private const string ChildSuffix = "_Part";
    private const int MaxPoolSize = 100; // Prevents uncontrolled pool growth

    #endregion

    #region Static Fields

    private static readonly Queue<LineRenderer> _pool = new Queue<LineRenderer>();
    private static Material _lineMaterial;

    private static GameObject _cubeParent;
    private static GameObject _cubeTempParent;
    private static GameObject _sphereParent;
    private static GameObject _sphereTempParent;

    private static bool _isQuitting = false;

    #endregion

    #region Public API

    public static void DestroyByName(string name)
    {
      if (_isQuitting) return;

      if (name == DebugShapeName.SphereTemp || name == DebugShapeName.CubeTemp)
        throw new System.InvalidOperationException("Destruction of temporary shapes is not allowed");

      foreach (var lr in Object.FindObjectsByType<LineRenderer>())
      {
        if (lr != null && lr.gameObject != null && lr.gameObject.name == name + ChildSuffix)
          Object.Destroy(lr.gameObject);
      }

      DestroyNonTempParents();
    }

    public static void DrawWireCube(Vector3 center, Vector3 size, Color color)
    {
      if (_isQuitting) return;

      EnsureParents(DebugShapeName.Cube);

      GameObject go = new GameObject(DebugShapeName.Cube + ChildSuffix);
      go.transform.SetParent(_cubeParent.transform, true);

      LineRenderer lr = go.AddComponent<LineRenderer>();
      SetupLineRenderer(lr, color);

      SetCubePositions(lr, center, size);
    }

    public static void DrawWireSphere(
      Vector3 center,
      float radius,
      Color color,
      int segments = DefaultSphereSegments)
    {
      if (_isQuitting) return;

      EnsureParents(DebugShapeName.Sphere);

      LineRenderer lr = GetPooledLineRenderer(DebugShapeName.Sphere + ChildSuffix, color);
      if (lr == null) return;

      lr.transform.SetParent(_sphereParent.transform, true);
      BuildWireSphere(center, radius, segments, lr);
    }

    public static void DrawTempWireCube(
      Vector3 center,
      Vector3 size,
      Color color,
      float duration = 1f)
    {
      if (_isQuitting) return;

      EnsureCoroutineRunner();
      EnsureParents(DebugShapeName.CubeTemp);

      LineRenderer lr = GetPooledLineRenderer(DebugShapeName.CubeTemp + ChildSuffix, color);
      if (lr == null) return;

      lr.transform.SetParent(_cubeTempParent.transform, true);
      SetCubePositions(lr, center, size);

      if (CoroutineRunner.Instance != null)
        CoroutineRunner.Instance.StartCoroutine(ReleaseAfter(lr, duration));
    }

    public static void DrawTempWireSphere(
      Vector3 center,
      float radius,
      Color color,
      int segments = DefaultSphereSegments,
      float duration = 1f)
    {
      if (_isQuitting) return;

      EnsureCoroutineRunner();
      EnsureParents(DebugShapeName.SphereTemp);

      LineRenderer lr = GetPooledLineRenderer(DebugShapeName.SphereTemp + ChildSuffix, color);
      if (lr == null) return;

      lr.transform.SetParent(_sphereTempParent.transform, true);
      BuildWireSphere(center, radius, segments, lr);

      if (CoroutineRunner.Instance != null)
        CoroutineRunner.Instance.StartCoroutine(ReleaseAfter(lr, duration));
    }

    /// <summary>
    /// Clears the entire pool and destroys all debug objects. Useful when transitioning between scenes.
    /// </summary>
    public static void Clear()
    {
      if (_isQuitting) return;

      // Clear the pool
      while (_pool.Count > 0)
      {
        var lr = _pool.Dequeue();
        if (lr != null && lr.gameObject != null)
          Object.Destroy(lr.gameObject);
      }

      // Destroy parent objects
      DestroyAllParents();
    }

    #endregion

    #region Shape Building

    private static void SetCubePositions(LineRenderer lr, Vector3 center, Vector3 size)
    {
      Vector3 h = size * 0.5f;
      Vector3[] v =
      {
        center + new Vector3(-h.x, -h.y, -h.z),
        center + new Vector3(h.x, -h.y, -h.z),
        center + new Vector3(h.x, -h.y, h.z),
        center + new Vector3(-h.x, -h.y, h.z),
        center + new Vector3(-h.x, h.y, -h.z),
        center + new Vector3(h.x, h.y, -h.z),
        center + new Vector3(h.x, h.y, h.z),
        center + new Vector3(-h.x, h.y, h.z),
      };

      Vector3[] positions =
      {
        v[0], v[1], v[1], v[2], v[2], v[3], v[3], v[0],
        v[4], v[5], v[5], v[6], v[6], v[7], v[7], v[4],
        v[0], v[4], v[1], v[5], v[2], v[6], v[3], v[7]
      };

      lr.positionCount = positions.Length;
      lr.SetPositions(positions);
    }

    private static void BuildWireSphere(
      Vector3 center,
      float radius,
      int segments,
      LineRenderer lr)
    {
      List<Vector3> positions = new List<Vector3>();

      int latitudeSegments = Mathf.Max(2, segments / 2);
      int longitudeSegments = Mathf.Max(3, segments);

      // Horizontal circles (latitudes)
      for (int lat = 1; lat < latitudeSegments; lat++)
      {
        float a = Mathf.PI * lat / latitudeSegments;
        float y = Mathf.Cos(a);
        float r = Mathf.Sin(a);

        AddCircle(
          center + Vector3.up * y * radius,
          Vector3.right,
          Vector3.forward,
          r * radius,
          longitudeSegments,
          positions
        );
      }

      // Vertical circles (longitudes)
      for (int lon = 0; lon < longitudeSegments; lon++)
      {
        float a = Mathf.PI * 2f * lon / longitudeSegments;
        Vector3 axisA = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
        Vector3 axisB = Vector3.up;

        AddCircle(
          center,
          axisA,
          axisB,
          radius,
          latitudeSegments * 2,
          positions
        );
      }

      lr.positionCount = positions.Count;
      lr.SetPositions(positions.ToArray());
    }

    private static void AddCircle(
      Vector3 center,
      Vector3 axisA,
      Vector3 axisB,
      float radius,
      int segments,
      List<Vector3> positions)
    {
      float step = Mathf.PI * 2f / segments;

      for (int i = 0; i <= segments; i++)
      {
        float a = step * i;
        positions.Add(center + (axisA * Mathf.Cos(a) + axisB * Mathf.Sin(a)) * radius);
      }
    }

    #endregion

    #region LineRenderer Management

    private static void SetupLineRenderer(LineRenderer lr, Color color, float width = 0.02f)
    {
      EnsureMaterial();

      lr.material = _lineMaterial;
      lr.startColor = lr.endColor = color;
      lr.startWidth = lr.endWidth = width;
      lr.loop = false;
      lr.positionCount = 0;
      lr.useWorldSpace = true;
      lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
      lr.receiveShadows = false;
    }

    private static LineRenderer GetPooledLineRenderer(string name, Color color, float width = 0.02f)
    {
      LineRenderer lr = null;

      // Solution 1: Clean the pool of destroyed objects
      while (_pool.Count > 0)
      {
        var pooled = _pool.Peek();

        // Check for null as Unity Object (handles destroyed objects)
        if (pooled == null)
        {
          _pool.Dequeue();
          continue;
        }

        // Additional GameObject check
        try
        {
          if (pooled.gameObject == null)
          {
            _pool.Dequeue();
            continue;
          }
        }
        catch
        {
          _pool.Dequeue();
          continue;
        }

        // Object is valid, use it
        lr = _pool.Dequeue();
        break;
      }

      // If we found a valid object in the pool
      if (lr != null)
      {
        lr.gameObject.SetActive(true);
      }
      else
      {
        // Create new object
        GameObject go = new GameObject(name);
        lr = go.AddComponent<LineRenderer>();

        EnsureMaterial();
        lr.material = _lineMaterial;
      }

      // Setup LineRenderer
      lr.gameObject.name = name;
      lr.startColor = lr.endColor = color;
      lr.startWidth = lr.endWidth = width;
      lr.loop = false;
      lr.positionCount = 0;
      lr.useWorldSpace = true;
      lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
      lr.receiveShadows = false;

      return lr;
    }

    private static void Release(LineRenderer lr)
    {
      if (lr == null) return;

      // Check that GameObject still exists
      try
      {
        if (lr.gameObject == null) return;
      }
      catch
      {
        return;
      }

      lr.gameObject.SetActive(false);

      // Limit pool size
      if (_pool.Count < MaxPoolSize)
      {
        _pool.Enqueue(lr);
      }
      else
      {
        // If pool is full, destroy the object
        Object.Destroy(lr.gameObject);
      }
    }

    private static IEnumerator ReleaseAfter(LineRenderer lr, float delay)
    {
      yield return new WaitForSeconds(delay);

      // Check that object wasn't destroyed during the wait
      if (lr != null && lr.gameObject != null)
      {
        Release(lr);
      }
    }

    #endregion

    #region Parent Management

    private static void EnsureParents(string name)
    {
      if (_isQuitting) return;

      switch (name)
      {
        case DebugShapeName.Sphere:
          if (_sphereParent == null)
          {
            _sphereParent = new GameObject(DebugShapeName.Sphere + ParentSuffix);
          }
          break;

        case DebugShapeName.Cube:
          if (_cubeParent == null)
          {
            _cubeParent = new GameObject(DebugShapeName.Cube + ParentSuffix);
          }
          break;

        // Solution 2: DontDestroyOnLoad for temporary parents
        case DebugShapeName.SphereTemp:
          if (_sphereTempParent == null)
          {
            _sphereTempParent = new GameObject(DebugShapeName.SphereTemp + ParentSuffix);
            Object.DontDestroyOnLoad(_sphereTempParent);
          }
          break;

        case DebugShapeName.CubeTemp:
          if (_cubeTempParent == null)
          {
            _cubeTempParent = new GameObject(DebugShapeName.CubeTemp + ParentSuffix);
            Object.DontDestroyOnLoad(_cubeTempParent);
          }
          break;
      }
    }

    private static void DestroyNonTempParents()
    {
      if (_sphereParent != null)
      {
        Object.Destroy(_sphereParent);
        _sphereParent = null;
      }

      if (_cubeParent != null)
      {
        Object.Destroy(_cubeParent);
        _cubeParent = null;
      }
    }

    private static void DestroyAllParents()
    {
      DestroyNonTempParents();

      if (_sphereTempParent != null)
      {
        Object.Destroy(_sphereTempParent);
        _sphereTempParent = null;
      }

      if (_cubeTempParent != null)
      {
        Object.Destroy(_cubeTempParent);
        _cubeTempParent = null;
      }
    }

    #endregion

    #region Initialization

    private static void EnsureCoroutineRunner()
    {
      if (_isQuitting) return;

      if (CoroutineRunner.Instance != null)
        return;

      GameObject go = new GameObject("DebugCoroutineRunner");
      Object.DontDestroyOnLoad(go);
      go.AddComponent<CoroutineRunner>();
    }

    private static void EnsureMaterial()
    {
      if (_lineMaterial == null)
      {
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
        {
          // Fallback to standard shader if Sprites/Default is not found
          shader = Shader.Find("Unlit/Color");
        }

        if (shader != null)
        {
          _lineMaterial = new Material(shader);
        }
        else
        {
          Debug.LogWarning("DrawDebugRuntime: Could not find suitable shader for line material");
        }
      }
    }

    #endregion

    #region Coroutine Runner

    // Solution 3: CoroutineRunner with pool cleanup on scene change
    private class CoroutineRunner : MonoBehaviour
    {
      public static CoroutineRunner Instance { get; private set; }

      private void Awake()
      {
        if (Instance == null)
        {
          Instance = this;
          DontDestroyOnLoad(gameObject);

          // Subscribe to scene change events
          UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
          UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneUnloaded;
        }
        else
        {
          Destroy(gameObject);
        }
      }

      private void OnDestroy()
      {
        if (Instance == this)
        {
          // Unsubscribe from events
          UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
          UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= OnSceneUnloaded;
          Instance = null;
        }
      }

      private void OnSceneLoaded(
        UnityEngine.SceneManagement.Scene scene,
        UnityEngine.SceneManagement.LoadSceneMode mode)
      {
        // Clean the pool of potentially destroyed objects
        CleanupPool();
      }

      private void OnSceneUnloaded(UnityEngine.SceneManagement.Scene scene)
      {
        // Additional cleanup when scene is unloaded
        CleanupPool();
      }

      private void OnApplicationQuit()
      {
        _isQuitting = true;
      }

      private void CleanupPool()
      {
        // Create a temporary list for valid objects
        List<LineRenderer> validRenderers = new List<LineRenderer>();

        while (_pool.Count > 0)
        {
          var lr = _pool.Dequeue();

          // Check if the object still exists
          if (lr != null && lr.gameObject != null)
          {
            try
            {
              // Additional check through activation
              bool isActive = lr.gameObject.activeSelf;
              validRenderers.Add(lr);
            }
            catch
            {
              // Object is destroyed, skip it
            }
          }
        }

        // Return valid objects back to the pool
        foreach (var lr in validRenderers)
        {
          _pool.Enqueue(lr);
        }
      }
    }

    #endregion
  }
}
