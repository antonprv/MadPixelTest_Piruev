// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEditor;
using UnityEditor.IMGUI.Controls;

using UnityEngine;
using UnityEngine.SceneManagement;

using Zenjex.Extensions.Runner;

namespace Reflex.Editor.DebuggingWindow
{
  public class ReflexDebuggerWindow : EditorWindow
  {
    // ── Tabs ───────────────────────────────────────────────────────────────────
    private static readonly string[] TabLabels = { "Reflex", "Zenjex" };
    [SerializeField] private int _activeTab;

    // ── Reflex tab ─────────────────────────────────────────────────────────────
    [SerializeField] private TreeViewState<int> _treeViewState;
    private ReflexTreeView _treeView;
    private SearchField _searchField;
    private Vector2 _reflexStackScroll;
    private Rect SearchBarRect => new Rect(20f, 38f, position.width - 40f, 20f);
    private Rect TreeViewRect => new Rect(20f, 58f, position.width - 40f, position.height - 210f);

    // ── Zenjex tab ─────────────────────────────────────────────────────────────
    private const float ZPanelW = 260f;
    private const float ZRowH = 20f;
    private Vector2 _zLeft, _zRight;
    private int _zSel = -1;
    private string _zSearchL = "", _zSearchR = "";
    private List<ZenjexRunner.InjectedRecord> _zRecords = new();
    private List<ZTypeMeta> _zTypes = new();

    // ── Menu ───────────────────────────────────────────────────────────────────
    //[MenuItem("Window/Analysis/Reflex Debugger %e")]
    //private static void Open() =>
    //  GetWindow<ReflexDebuggerWindow>(false, "Reflex Debugger", true);

    // ── Lifecycle ──────────────────────────────────────────────────────────────
    private void OnEnable()
    {
      SceneManager.sceneLoaded += OnSceneLoaded;
      SceneManager.sceneUnloaded += OnSceneUnloaded;
      EditorApplication.playModeStateChanged += OnPlayMode;
      ZenjexRunner.OnStateChanged += OnZenjexChanged;
      RefreshReflex();
      RefreshZenjex();
    }

    private void OnDisable()
    {
      SceneManager.sceneLoaded -= OnSceneLoaded;
      SceneManager.sceneUnloaded -= OnSceneUnloaded;
      EditorApplication.playModeStateChanged -= OnPlayMode;
      ZenjexRunner.OnStateChanged -= OnZenjexChanged;
    }

    private void OnSceneLoaded(Scene s, LoadSceneMode m) => EditorApplication.delayCall += DelayRefresh;
    private void OnSceneUnloaded(Scene s) => EditorApplication.delayCall += DelayRefresh;
    private void OnPlayMode(PlayModeStateChange _) => EditorApplication.delayCall += DelayRefresh;
    private void OnZenjexChanged() => RefreshZenjex();

    private void DelayRefresh()
    {
      EditorApplication.delayCall -= DelayRefresh;
      RefreshReflex();
      RefreshZenjex();
    }

    // ── OnGUI ──────────────────────────────────────────────────────────────────
    private void OnGUI()
    {
      EditorGUI.BeginChangeCheck();
      _activeTab = GUILayout.Toolbar(_activeTab, TabLabels, EditorStyles.toolbarButton, GUILayout.Height(24));
      if (EditorGUI.EndChangeCheck()) Repaint();

      if (_activeTab == 0) DrawReflexTab();
      else DrawZenjexTab();
    }

    // ══════════════════════════════════════════════════════════════════════════
    // REFLEX TAB
    // ══════════════════════════════════════════════════════════════════════════

    private void RefreshReflex()
    {
      if (_treeViewState == null) _treeViewState = new TreeViewState<int>();

      var header = ReflexTreeView.CreateHeader();
      _treeView = new ReflexTreeView(_treeViewState, header);

      var (roots, nodes) = ReflexTreeViewBuilder.Build(
        ReflexEditorSettings.ShowInternalBindings,
        ReflexEditorSettings.ShowInheritedBindings);
      _treeView.SetData(roots, nodes);

      _searchField = new SearchField();
      _searchField.downOrUpArrowKeyPressed += _treeView.SetFocusAndEnsureSelectedItem;

      Repaint();
    }

    private void DrawReflexTab()
    {
      if (_treeView == null || _searchField == null)
      {
        RefreshReflex();
        if (_treeView == null || _searchField == null)
          return;
      }

      if (UnityScriptingDefineSymbols.IsDefined("REFLEX_DEBUG"))
        DrawReflexEnabled();
      else
        DrawReflexDisabled();

      GUILayout.FlexibleSpace();
      DrawReflexStatusBar();
    }

    private static void DrawReflexDisabled()
    {
      GUILayout.FlexibleSpace();
      GUILayout.Label("To start debugging, enable Reflex Debug Mode in the status bar.", Styles.LabelHorizontallyCentered);
      GUILayout.Label("Keep in mind that enabling Reflex Debug Mode will impact performance.", Styles.LabelHorizontallyCentered);
    }

    private void DrawReflexEnabled()
    {
      if (_treeView == null || _searchField == null)
        return;

      _treeView.searchString = _searchField.OnGUI(SearchBarRect, _treeView.searchString);
      GUILayoutUtility.GetRect(SearchBarRect.width, SearchBarRect.height);

      _treeView.OnGUI(TreeViewRect);
      GUILayoutUtility.GetRect(TreeViewRect.width, TreeViewRect.height);

      GUILayout.Space(16);
      using (new GUILayout.HorizontalScope())
      {
        GUILayout.Space(16);
        using (new GUILayout.VerticalScope())
        {
          _reflexStackScroll = GUILayout.BeginScrollView(_reflexStackScroll);
          DrawReflexCallSite();
          GUILayout.EndScrollView();
          GUILayout.Space(16);
        }
        GUILayout.Space(16);
      }
    }

    private void DrawReflexStatusBar()
    {
      using (new EditorGUILayout.HorizontalScope(Styles.AppToolbar))
      {
        GUILayout.FlexibleSpace();
        EditorGUI.BeginChangeCheck();
        ReflexEditorSettings.ShowInternalBindings = GUILayout.Toggle(ReflexEditorSettings.ShowInternalBindings, "Show Internal Bindings ");
        ReflexEditorSettings.ShowInheritedBindings = GUILayout.Toggle(ReflexEditorSettings.ShowInheritedBindings, "Show Inherited Bindings ");
        if (EditorGUI.EndChangeCheck()) RefreshReflex();

        var refreshIcon = EditorGUIUtility.IconContent("d_TreeEditor.Refresh");
        refreshIcon.tooltip = "Force refresh";
        if (GUILayout.Button(refreshIcon, Styles.StatusBarIcon, GUILayout.Width(25)))
          RefreshReflex();

        var isDebug = UnityScriptingDefineSymbols.IsDefined("REFLEX_DEBUG");
        var debugIcon = EditorGUIUtility.IconContent(isDebug ? "d_DebuggerEnabled" : "d_DebuggerDisabled");
        debugIcon.tooltip = isDebug ? "Reflex Debugger Enabled" : "Reflex Debugger Disabled";
        if (GUILayout.Button(debugIcon, Styles.StatusBarIcon, GUILayout.Width(25)))
          UnityScriptingDefineSymbols.Toggle("REFLEX_DEBUG");
      }
    }

    private void DrawReflexCallSite()
    {
      var sel = _treeView.GetSelection();
      if (sel == null || sel.Count == 0) return;

      var node = _treeView.FindNode(sel.First());
      if (node?.Callsite == null) return;

      foreach (var cs in node.Callsite)
      {
        using (new GUILayout.HorizontalScope())
        {
          GUILayout.Label($"{cs.ClassName}:{cs.FunctionName}()  \u2192", Styles.StackTrace);
          if (DrawLinkButton($"{cs.Path}:{cs.Line}"))
            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(cs.Path, cs.Line);
        }
      }
    }

    private static bool DrawLinkButton(string label, params GUILayoutOption[] opts)
    {
      var pos = GUILayoutUtility.GetRect(new GUIContent(label), Styles.Hyperlink, opts);
      pos.y -= 3;
      Handles.color = Styles.Hyperlink.normal.textColor;
      Handles.DrawLine(
        new Vector3(pos.xMin + EditorStyles.linkLabel.padding.left, pos.yMax),
        new Vector3(pos.xMax - EditorStyles.linkLabel.padding.right, pos.yMax));
      Handles.color = Color.white;
      EditorGUIUtility.AddCursorRect(pos, MouseCursor.Link);
      return GUI.Button(pos, label, Styles.Hyperlink);
    }

    // ══════════════════════════════════════════════════════════════════════════
    // ZENJEX TAB
    // ══════════════════════════════════════════════════════════════════════════

    private void RefreshZenjex()
    {
      _zRecords = new List<ZenjexRunner.InjectedRecord>(ZenjexRunner.InjectedRecords);
      _zTypes = BuildZTypeMeta();
      _zSel = -1;
      Repaint();
    }

    private void DrawZenjexTab()
    {
      // Status strip
      using (new EditorGUILayout.HorizontalScope(Styles.AppToolbar))
      {
        var ready = Application.isPlaying && ZenjexRunner.IsReady;
        GUILayout.Label(ready ? "\u25cf Container Ready" : "\u25cf Not Running",
          ready ? ZenjexStyles.StatusGreen : ZenjexStyles.StatusGray);
        GUILayout.Space(6);
        if (Application.isPlaying)
        {
          GUILayout.Label($"Injected: {_zRecords.Count}", EditorStyles.miniLabel);
          var late = _zRecords.Count(r => r.IsLate);
          if (late > 0) { GUILayout.Space(4); GUILayout.Label($"\u26a0 Late: {late}", ZenjexStyles.StatusWarning); }
        }
        GUILayout.FlexibleSpace();
        var ri = EditorGUIUtility.IconContent("d_TreeEditor.Refresh"); ri.tooltip = "Refresh";
        if (GUILayout.Button(ri, Styles.StatusBarIcon, GUILayout.Width(25))) RefreshZenjex();
      }

      using (new GUILayout.HorizontalScope())
      {
        // Left panel
        using (new GUILayout.VerticalScope(GUILayout.Width(ZPanelW)))
        {
          using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
          {
            GUILayout.Label("Injected Objects", EditorStyles.boldLabel);
            _zSearchL = EditorGUILayout.TextField(_zSearchL, EditorStyles.toolbarSearchField, GUILayout.Width(110));
          }

          _zLeft = GUILayout.BeginScrollView(_zLeft);
          if (!Application.isPlaying)
            DrawHint("Enter Play Mode to see injected objects.");
          else if (_zRecords.Count == 0)
            DrawHint("No objects injected yet.");
          else
          {
            for (var i = 0; i < _zRecords.Count; i++)
            {
              var r = _zRecords[i];
              if (!ZMatch(r.TypeName + r.GoName + r.SceneName, _zSearchL)) continue;

              var selected = _zSel == i;
              var bg = selected ? ZenjexStyles.RowSelected
                : (i % 2 == 0 ? ZenjexStyles.RowEven : ZenjexStyles.RowOdd);
              var row = GUILayoutUtility.GetRect(ZPanelW, ZRowH, bg);

              if (Event.current.type == EventType.MouseDown && row.Contains(Event.current.mousePosition))
              { _zSel = selected ? -1 : i; Event.current.Use(); Repaint(); }

              if (Event.current.type == EventType.Repaint)
              {
                bg.Draw(row, false, false, selected, false);
                EditorGUI.DrawRect(
                  new Rect(row.x + 4, row.y + (row.height - 10) * 0.5f, 10, 10),
                  ZPassColor(r.Pass));
                var col = r.IsLate ? ZenjexStyles.LateColor : ZenjexStyles.NormalColor;
                var lbl = $"<color=#{ColorUtility.ToHtmlStringRGB(col)}>{r.TypeName}</color>" +
                           $"  <color=#666666>{r.GoName}</color>";
                ZenjexStyles.RichLabel.Draw(
                  new Rect(row.x + 18, row.y, row.width - 18, row.height),
                  new GUIContent(lbl), false, false, selected, false);
              }
            }
          }
          GUILayout.EndScrollView();
        }

        // Divider
        EditorGUI.DrawRect(GUILayoutUtility.GetRect(1, float.MaxValue, GUILayout.Width(1)),
          new Color(0.15f, 0.15f, 0.15f));

        // Right panel
        using (new GUILayout.VerticalScope())
        {
          if (_zSel >= 0 && _zSel < _zRecords.Count) DrawZDetail(_zRecords[_zSel]);
          else DrawZTypeBrowser();
        }
      }
    }

    private void DrawZDetail(ZenjexRunner.InjectedRecord r)
    {
      using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
        GUILayout.Label(r.TypeName, EditorStyles.boldLabel);

      _zRight = GUILayout.BeginScrollView(_zRight);

      void Row(string l, string v)
      {
        using (new GUILayout.HorizontalScope())
        {
          GUILayout.Label(l, EditorStyles.boldLabel, GUILayout.Width(56));
          GUILayout.Label(v, ZenjexStyles.RichLabel);
        }
      }
      Row("Type", r.TypeName);
      Row("Object", r.GoName);
      Row("Scene", r.SceneName);
      Row("Pass", ZPassLabel(r.Pass));
      Row("Late", r.IsLate ? "\u26a0 Yes \u2014 Awake() saw nulls" : "No");

      GUILayout.Space(10);
      GUILayout.Label("[Zenjex] Members", EditorStyles.boldLabel);
      GUILayout.Space(2);

      var meta = _zTypes.Find(m => m.TypeName == r.TypeName);
      if (meta != null)
        foreach (var m in meta.Members)
          GUILayout.Label(
            $"  <color=#888888>[{m.Kind}]</color>  <b>{m.Name}</b>  <color=#555555>{m.TypeName}</color>",
            ZenjexStyles.RichLabel);

      GUILayout.EndScrollView();
    }

    private void DrawZTypeBrowser()
    {
      using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
      {
        GUILayout.Label("Types with [Zenjex]", EditorStyles.boldLabel);
        _zSearchR = EditorGUILayout.TextField(_zSearchR, EditorStyles.toolbarSearchField, GUILayout.Width(110));
      }

      _zRight = GUILayout.BeginScrollView(_zRight);
      if (_zTypes.Count == 0)
        DrawHint("No [Zenjex] types found.");
      else
        foreach (var meta in _zTypes)
        {
          if (!ZMatch(meta.TypeName + meta.AssemblyName, _zSearchR)) continue;
          GUILayout.Label(
            $"<b>{meta.TypeName}</b>  <color=#555555>{meta.AssemblyName}</color>",
            ZenjexStyles.RichLabel);
          foreach (var m in meta.Members)
            GUILayout.Label(
              $"  <color=#888888>[{m.Kind}]</color>  <color=#AAAAAA>{m.Name}</color>  <color=#555555>{m.TypeName}</color>",
              ZenjexStyles.RichLabel);
          GUILayout.Space(4);
        }
      GUILayout.EndScrollView();
    }

    private static void DrawHint(string msg)
    {
      GUILayout.FlexibleSpace();
      GUILayout.Label(msg, Styles.LabelHorizontallyCentered);
      GUILayout.FlexibleSpace();
    }

    private static bool ZMatch(string text, string q) =>
      string.IsNullOrEmpty(q) || text.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0;

    private static string ZPassLabel(ZenjexRunner.InjectionPass p) => p switch
    {
      ZenjexRunner.InjectionPass.ContainerReady => "Pass 1 \u2014 Container Ready",
      ZenjexRunner.InjectionPass.GameLaunched => "Pass 2 \u2014 Game Launched",
      ZenjexRunner.InjectionPass.SceneLoaded => "Pass 3 \u2014 Scene Loaded (late)",
      ZenjexRunner.InjectionPass.Manual => "Manual \u2014 InjectGameObject()",
      ZenjexRunner.InjectionPass.ZenjexBehaviour => "ZenjexBehaviour.Awake()",
      _ => p.ToString()
    };

    private static Color ZPassColor(ZenjexRunner.InjectionPass p) => p switch
    {
      ZenjexRunner.InjectionPass.ContainerReady => new Color(0.3f, 0.85f, 0.4f),
      ZenjexRunner.InjectionPass.GameLaunched => new Color(0.3f, 0.6f, 0.9f),
      ZenjexRunner.InjectionPass.SceneLoaded => new Color(0.9f, 0.65f, 0.2f),
      ZenjexRunner.InjectionPass.Manual => new Color(0.8f, 0.5f, 0.9f),
      ZenjexRunner.InjectionPass.ZenjexBehaviour => new Color(0.2f, 0.9f, 0.7f),
      _ => Color.gray
    };

    // ── Zenjex type scanner ────────────────────────────────────────────────────

    private static List<ZTypeMeta> BuildZTypeMeta()
    {
      const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                                 BindingFlags.Instance | BindingFlags.DeclaredOnly;
      var result = new List<ZTypeMeta>();

      foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
      {
        if (IsSystemAsm(asm)) continue;
        Type[] types;
        try { types = asm.GetTypes(); }
        catch { continue; }

        foreach (var type in types)
        {
          if (!type.IsClass || type.IsAbstract || !typeof(MonoBehaviour).IsAssignableFrom(type)) continue;
          var members = new List<ZMemberMeta>();
          for (var t = type; t != null && t != typeof(object); t = t.BaseType)
          {
            foreach (var f in t.GetFields(flags))
              if (f.IsDefined(typeof(Zenjex.Extensions.Attribute.ZenjexAttribute)))
                members.Add(new ZMemberMeta(f.Name, f.FieldType.Name, "F"));
            foreach (var pp in t.GetProperties(flags))
              if (pp.IsDefined(typeof(Zenjex.Extensions.Attribute.ZenjexAttribute)))
                members.Add(new ZMemberMeta(pp.Name, pp.PropertyType.Name, "P"));
            foreach (var m in t.GetMethods(flags))
              if (m.IsDefined(typeof(Zenjex.Extensions.Attribute.ZenjexAttribute)))
                members.Add(new ZMemberMeta(m.Name, "void", "M"));
          }
          if (members.Count > 0) result.Add(new ZTypeMeta(type.Name, asm.GetName().Name, members));
        }
      }

      result.Sort((a, b) => string.Compare(a.TypeName, b.TypeName, StringComparison.Ordinal));
      return result;
    }

    private static bool IsSystemAsm(Assembly a)
    {
      var n = a.GetName().Name;
      return n.StartsWith("System") || n.StartsWith("mscorlib") || n.StartsWith("UnityEngine") ||
             n.StartsWith("UnityEditor") || n.StartsWith("Unity.") || n.StartsWith("Mono.") ||
             n.StartsWith("netstandard");
    }

    private sealed class ZMemberMeta { public readonly string Name, TypeName, Kind; public ZMemberMeta(string n, string t, string k) { Name = n; TypeName = t; Kind = k; } }
    private sealed class ZTypeMeta { public readonly string TypeName, AssemblyName; public readonly List<ZMemberMeta> Members; public ZTypeMeta(string t, string a, List<ZMemberMeta> m) { TypeName = t; AssemblyName = a; Members = m; } }
  }
}
