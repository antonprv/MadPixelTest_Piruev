// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System;
using System.Collections.Generic;
using System.Linq;

using Reflex.Core;
using Reflex.Extensions;
using Reflex.Resolvers;

using UnityEditor;
using UnityEditor.IMGUI.Controls;

using UnityEngine;

namespace Reflex.Editor.DebuggingWindow
{
  // ── Node data ───────────────────────────────────────────────────────────────

  internal sealed class ReflexNode
  {
    public string Label;        // rich-text display name
    public string[] Contracts;    // shown as badges in Hierarchy column
    public string Lifetime;
    public string ResolverKind;
    public Func<string> Resolutions;  // live counter - only set for resolvers
    public List<CallSite> Callsite;
    public Texture Icon;
  }

  // ── TreeView<int> ────────────────────────────────────────────────────────────
  // Unity 6 replaced the non-generic TreeView/TreeViewItem/TreeViewState with
  // generic versions: TreeView<TId>, TreeViewItem<TId>, TreeViewState<TId>.
  // We use int as the identifier type.

  internal sealed class ReflexTreeView : TreeView<int>
  {
    private const float RowH = 20f;
    private const float ToggleW = 18f;

    private enum Col { Hierarchy, Kind, Lifetime, Calls }

    // All node data keyed by item id - populated externally via SetData().
    private Dictionary<int, ReflexNode> _nodes = new();

    // Pending tree that BuildRoot will consume on the next Reload().
    private TreeViewItem<int> _pendingRoot;

    // ── Constructor ───────────────────────────────────────────────────────────

    public ReflexTreeView(TreeViewState<int> state, MultiColumnHeader header)
      : base(state, header)
    {
      rowHeight = RowH;
      columnIndexForTreeFoldouts = 0;
      showAlternatingRowBackgrounds = true;
      showBorder = true;
      customFoldoutYOffset = (RowH - EditorGUIUtility.singleLineHeight) * 0.5f;
      extraSpaceBeforeIconAndLabel = ToggleW;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Feed new data and rebuild the tree.</summary>
    public void SetData(TreeViewItem<int> root, Dictionary<int, ReflexNode> nodes)
    {
      _pendingRoot = root;
      _nodes = nodes;
      Reload();   // triggers BuildRoot → BuildRows
      ExpandAll();
    }

    /// <summary>Return node data for a given item id (used by callsite panel).</summary>
    public ReflexNode FindNode(int id) =>
      _nodes.TryGetValue(id, out var n) ? n : null;

    // ── TreeView<int> overrides ───────────────────────────────────────────────

    protected override TreeViewItem<int> BuildRoot()
    {
      if (_pendingRoot == null)
      {
        var empty = new TreeViewItem<int>(0, -1, "Root")
        {
          children = new List<TreeViewItem<int>>()   // ← ВАЖНО
        };

        SetupDepthsFromParentsAndChildren(empty);
        return empty;
      }

      // Ensure root has children list
      if (_pendingRoot.children == null)
        _pendingRoot.children = new List<TreeViewItem<int>>();

      SetupDepthsFromParentsAndChildren(_pendingRoot);
      return _pendingRoot;
    }


    protected override void RowGUI(RowGUIArgs args)
    {
      if (!_nodes.TryGetValue(args.item.id, out var node))
      {
        base.RowGUI(args);
        return;
      }

      for (var i = 0; i < args.GetNumVisibleColumns(); i++)
        DrawCell(args.GetCellRect(i), args.item, node, (Col)args.GetColumn(i));
    }

    protected override bool CanMultiSelect(TreeViewItem<int> item) => false;

    // ── Cell rendering ────────────────────────────────────────────────────────

    private void DrawCell(Rect cellRect, TreeViewItem<int> item, ReflexNode node, Col col)
    {
      CenterRectUsingSingleLineHeight(ref cellRect);

      switch (col)
      {
        case Col.Hierarchy:
          var indent = GetContentIndent(item);

          // Icon
          if (node.Icon != null)
          {
            GUI.BeginGroup(cellRect);
            GUI.DrawTexture(new Rect(indent, 0, 16, cellRect.height), node.Icon, ScaleMode.ScaleToFit);
            GUI.EndGroup();
          }

          var textRect = cellRect;
          textRect.xMin += indent + 20;

          if (node.Contracts != null && node.Contracts.Length > 0)
            DrawContractBadges(textRect, node.Contracts);
          else
            GUI.Label(textRect, node.Label, Styles.RichTextLabel);
          break;

        case Col.Calls:
          GUI.Label(cellRect, node.Resolutions?.Invoke() ?? "");
          break;

        case Col.Kind:
          GUI.Label(cellRect, node.ResolverKind ?? "");
          break;

        case Col.Lifetime:
          GUI.Label(cellRect, node.Lifetime ?? "");
          break;
      }
    }

    private static void DrawContractBadges(Rect rect, string[] contracts)
    {
      rect.y += 1;
      var style = new GUIStyle("CN CountBadge")
      {
        wordWrap = false,
        stretchWidth = false,
        stretchHeight = false,
        fontStyle = FontStyle.Bold,
        fontSize = 11
      };

      GUI.BeginGroup(rect);
      var xOff = 0f;
      foreach (var contract in contracts)
      {
        var content = new GUIContent(contract);
        var w = style.CalcSize(content).x;
        GUI.Label(new Rect(xOff, 0, w, rect.height), content, style);
        xOff += w + 4;
        if (xOff > rect.width) break;
      }
      GUI.EndGroup();
    }

    // ── Column header factory ─────────────────────────────────────────────────

    public static MultiColumnHeader CreateHeader()
    {
      var columns = new[]
      {
        new MultiColumnHeaderState.Column
        {
          canSort = false, headerContent = new GUIContent(nameof(Col.Hierarchy)),
          headerTextAlignment = TextAlignment.Left,
          width = 280, minWidth = 60, autoResize = true, allowToggleVisibility = false
        },
        new MultiColumnHeaderState.Column
        {
          canSort = false, headerContent = new GUIContent(nameof(Col.Kind)),
          headerTextAlignment = TextAlignment.Left,
          width = 64, minWidth = 64, maxWidth = 64, autoResize = false, allowToggleVisibility = false
        },
        new MultiColumnHeaderState.Column
        {
          canSort = false, headerContent = new GUIContent(nameof(Col.Lifetime)),
          headerTextAlignment = TextAlignment.Left,
          width = 64, minWidth = 64, maxWidth = 64, autoResize = false, allowToggleVisibility = false
        },
        new MultiColumnHeaderState.Column
        {
          canSort = false, headerContent = new GUIContent(nameof(Col.Calls)),
          headerTextAlignment = TextAlignment.Left,
          width = 38, minWidth = 38, autoResize = false
        },
      };

      var header = new MultiColumnHeader(new MultiColumnHeaderState(columns))
      {
        canSort = false,
        height = MultiColumnHeader.DefaultGUI.minimumHeight
      };
      header.ResizeToFit();
      return header;
    }
  }

  // ── Builder ──────────────────────────────────────────────────────────────────
  // Converts the Container hierarchy into a TreeViewItem<int> tree + node dict.

  internal static class ReflexTreeViewBuilder
  {
    private const string ContainerIcon = "PreMatCylinder";
    private const string ResolverIcon = "d_NetworkAnimator Icon";
    private const string InstanceIcon = "d_Prefab Icon";

    public static (TreeViewItem<int> root, Dictionary<int, ReflexNode> nodes) Build(
      bool showInternal, bool showInherited)
    {
      var nodes = new Dictionary<int, ReflexNode>();
      var nextId = 1; // 0 reserved for hidden root

      var root = new TreeViewItem<int>(0, -1, "Root")
      {
        children = new List<TreeViewItem<int>>()
      };

      foreach (var container in Container.RootContainers)
        root.AddChild(BuildContainer(container, nodes, ref nextId, showInternal, showInherited));

      return (root, nodes);
    }

    private static TreeViewItem<int> BuildContainer(
      Container container,
      Dictionary<int, ReflexNode> nodes,
      ref int nextId,
      bool showInternal,
      bool showInherited)
    {
      var id = nextId++;
      var item = new TreeViewItem<int>(id, -1, container.Name); // depth set by SetupDepths later

      nodes[id] = new ReflexNode
      {
        Label = container.Name,
        Icon = EditorGUIUtility.IconContent(ContainerIcon).image,
        Callsite = container.GetDebugProperties().BuildCallsite,
      };

      foreach (var (resolver, contracts) in GetMatrix(container, showInternal, showInherited))
      {
        var rId = nextId++;
        var rItem = new TreeViewItem<int>(rId, -1, string.Join(", ", contracts.Select(x => x.GetName())));
        item.AddChild(rItem);

        nodes[rId] = new ReflexNode
        {
          Label = rItem.displayName,
          Contracts = contracts.Select(x => x.GetName()).OrderBy(x => x).ToArray(),
          Lifetime = resolver.Lifetime.ToString(),
          ResolverKind = resolver.GetType().Name
            .Replace("Singleton", "").Replace("Transient", "")
            .Replace("Scoped", "").Replace("Resolver", ""),
          Resolutions = () => resolver.GetDebugProperties().Resolutions.ToString(),
          Icon = EditorGUIUtility.IconContent(ResolverIcon).image,
          Callsite = resolver.GetDebugProperties().BindingCallsite,
        };

        foreach (var (instance, callsite) in resolver.GetDebugProperties().Instances
          .Where(t => t.Item1.IsAlive)
          .Select(t => (t.Item1.Target, t.Item2)))
        {
          var iId = nextId++;
          var label = $"{instance.GetType().GetName()} <color=#3D99ED>({SHA1.ShortHash(instance.GetHashCode())})</color>";
          var iItem = new TreeViewItem<int>(iId, -1, label);
          rItem.AddChild(iItem);

          nodes[iId] = new ReflexNode
          {
            Label = label,
            Icon = EditorGUIUtility.IconContent(InstanceIcon).image,
            Callsite = callsite,
          };
        }
      }

      foreach (var child in container.Children)
        item.AddChild(BuildContainer(child, nodes, ref nextId, showInternal, showInherited));

      return item;
    }

    private static IEnumerable<(IResolver, Type[])> GetMatrix(
      Container c, bool showInternal, bool showInherited)
    {
      var resolvers = c.ResolversByContract.Values.SelectMany(r => r).ToHashSet();

      if (!showInternal && c.TryGetResolver(typeof(Container), out var self))
        resolvers.Remove(self);

      if (!showInherited && c.Parent != null)
        foreach (var pr in c.Parent.ResolversByContract.Values.SelectMany(r => r).Distinct())
          resolvers.Remove(pr);

      return resolvers.Select(r =>
        (r, c.ResolversByContract.Where(p => p.Value.Contains(r)).Select(p => p.Key).ToArray()));
    }
  }
}
