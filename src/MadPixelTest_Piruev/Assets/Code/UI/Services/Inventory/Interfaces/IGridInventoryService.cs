// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

using Code.Core;
using Code.Data.StaticData;

using R3;

using UnityEngine;

namespace Code.Services.Interfaces
{
  public interface IGridInventoryService
  {
    // ─── Events ───────────────────────────────────────────────────────────────
    Observable<InventoryItem> OnItemPlaced { get; }
    Observable<InventoryItem> OnItemRemoved { get; }
    Observable<MergeResult> OnItemsMerged { get; }

    // ─── Placement ────────────────────────────────────────────────────────────
    bool CanPlace(ItemConfig config, Vector2Int origin, InventoryItem ignore = null);
    bool TryPlace(InventoryItem item);
    bool TryRemove(InventoryItem item);

    // ─── Query ────────────────────────────────────────────────────────────────
    InventoryItem GetItemAt(Vector2Int cell);
    IReadOnlyList<InventoryItem> GetAllItems();

    // ─── Merge ────────────────────────────────────────────────────────────────
    bool CanMerge(InventoryItem dragged, Vector2Int targetCell, out InventoryItem targetItem);
    InventoryItem Merge(InventoryItem a, InventoryItem b);
  }

  public readonly struct MergeResult
  {
    public readonly InventoryItem A;
    public readonly InventoryItem B;
    public readonly InventoryItem Result;

    public MergeResult(InventoryItem a, InventoryItem b, InventoryItem result)
    {
      A = a; B = b; Result = result;
    }
  }
}
