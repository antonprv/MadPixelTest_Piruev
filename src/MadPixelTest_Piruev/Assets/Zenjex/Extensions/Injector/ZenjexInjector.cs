// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

using Zenjex.Extensions.Attribute;
using Zenjex.Extensions.Core;

namespace Zenjex.Extensions.Injector
{
  /// <summary>
  /// Core injection logic. Resolves all [Zenjex]-marked members on a given object
  /// from RootContext. Called by ZenjexRunner automatically before Awake.
  /// </summary>
  public static class ZenjexInjector
  {
    private const BindingFlags Flags =
      BindingFlags.Public | BindingFlags.NonPublic |
      BindingFlags.Instance | BindingFlags.DeclaredOnly;

    private static readonly Dictionary<Type, TypeZenjexInfo> _cache = new();

    // ── Public entry point ────────────────────────────────────────────────────

    public static void Inject(object target)
    {
      var info = GetInfo(target.GetType());

      if (!info.HasAnyMembers)
        return;

      foreach (var field in info.Fields)
      {
        try { field.SetValue(target, RootContext.Resolve(field.FieldType)); }
        catch (Exception ex) { LogError(target, $"field '{field.Name}' ({field.FieldType.Name})", ex); }
      }

      foreach (var prop in info.Properties)
      {
        try { prop.SetValue(target, RootContext.Resolve(prop.PropertyType)); }
        catch (Exception ex) { LogError(target, $"property '{prop.Name}' ({prop.PropertyType.Name})", ex); }
      }

      foreach (var method in info.Methods)
      {
        try
        {
          var parameters = method.GetParameters();
          var args = new object[parameters.Length];
          for (var i = 0; i < parameters.Length; i++)
            args[i] = RootContext.Resolve(parameters[i].ParameterType);
          method.Invoke(target, args);
        }
        catch (Exception ex) { LogError(target, $"method '{method.Name}'", ex); }
      }
    }

    // ── Public helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// Returns true if the type has any [Zenjex]-marked members.
    /// Used by ZenjexRunner to skip unrelated MonoBehaviours cheaply.
    /// </summary>
    public static bool HasZenjexMembers(Type type) => GetInfo(type).HasAnyMembers;

    // ── Cache ─────────────────────────────────────────────────────────────────

    private static TypeZenjexInfo GetInfo(Type type)
    {
      if (!_cache.TryGetValue(type, out var info))
      {
        info = BuildInfo(type);
        _cache[type] = info;
      }
      return info;
    }

    private static TypeZenjexInfo BuildInfo(Type type)
    {
      var fields = new List<FieldInfo>();
      var props = new List<PropertyInfo>();
      var methods = new List<MethodInfo>();

      var t = type;
      while (t != null && t != typeof(object))
      {
        foreach (var f in t.GetFields(Flags))
          if (f.IsDefined(typeof(ZenjexAttribute)))
            fields.Add(f);

        foreach (var p in t.GetProperties(Flags))
          if (p.CanWrite && p.IsDefined(typeof(ZenjexAttribute)))
            props.Add(p);

        foreach (var m in t.GetMethods(Flags))
          if (m.IsDefined(typeof(ZenjexAttribute)))
            methods.Add(m);

        t = t.BaseType;
      }

      return new TypeZenjexInfo(fields.ToArray(), props.ToArray(), methods.ToArray());
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void LogError(object target, string member, Exception ex) =>
      Debug.LogError($"[Zenjex] Failed to resolve {member} on {target.GetType().Name}: {ex.Message}");

    // ── Cache DTO ─────────────────────────────────────────────────────────────

    private sealed class TypeZenjexInfo
    {
      public readonly FieldInfo[] Fields;
      public readonly PropertyInfo[] Properties;
      public readonly MethodInfo[] Methods;
      public readonly bool HasAnyMembers;

      public TypeZenjexInfo(FieldInfo[] fields, PropertyInfo[] properties, MethodInfo[] methods)
      {
        Fields = fields;
        Properties = properties;
        Methods = methods;
        HasAnyMembers = fields.Length > 0 || properties.Length > 0 || methods.Length > 0;
      }
    }
  }
}
