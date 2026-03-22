// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

using UnityEditor;
using UnityEditor.Build;

public static class UnityScriptingDefineSymbols
{
  public static bool IsDefined(string symbol)
  {
    return GetSymbols().Contains(symbol);
  }

  public static void Add(string symbol)
  {
    var symbols = GetSymbols();
    if (symbols.Add(symbol))
      SetSymbols(symbols);
  }

  public static void Remove(string symbol)
  {
    var symbols = GetSymbols();
    if (symbols.Remove(symbol))
      SetSymbols(symbols);
  }

  public static void Toggle(string symbol)
  {
    var symbols = GetSymbols();
    if (!symbols.Remove(symbol))
      symbols.Add(symbol);
    SetSymbols(symbols);
  }

  // Legacy overloads kept for source compatibility - BuildTargetGroup param is ignored,
  // we always operate on the active named build target instead.
  // BuildTargetGroup itself is not obsolete, but selectedBuildTargetGroup is in Unity 6.
  public static void Add(string symbol, BuildTargetGroup _) => Add(symbol);
  public static void Remove(string symbol, BuildTargetGroup _) => Remove(symbol);
  public static void Toggle(string symbol, BuildTargetGroup _) => Toggle(symbol);

  // ── Internals ──────────────────────────────────────────────────────────────

  private static NamedBuildTarget ActiveTarget =>
    NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

  private static HashSet<string> GetSymbols()
  {
    var raw = PlayerSettings.GetScriptingDefineSymbols(ActiveTarget);
    return new HashSet<string>(raw.Split(new[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries));
  }

  private static void SetSymbols(HashSet<string> symbols)
  {
    PlayerSettings.SetScriptingDefineSymbols(ActiveTarget, string.Join(";", symbols));
  }
}
