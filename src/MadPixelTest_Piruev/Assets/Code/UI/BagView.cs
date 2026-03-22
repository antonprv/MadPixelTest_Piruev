using System;
using System.Collections.Generic;
using R3;
using UnityEngine;
using BagFight.Core;
using BagFight.Data;
using BagFight.Services.Interfaces;
using BagFight.UI.Types;
using Zenjex.Extensions.Attribute;
using Zenjex.Extensions.Injector;

namespace BagFight.UI
{
  /// <summary>
  /// Спавнит сетку CellView по BagConfig и реагирует на события инвентаря.
  ///
  /// Highlights:
  ///   При OnPointerEnter в любой CellView → тот вызывает HighlightItem()
  ///   BagView подсвечивает все ячейки формы предмета в нужный цвет.
  /// </summary>
  public class BagView : ZenjexBehaviour
  {
    [SerializeField] private RectTransform _gridRoot;
    [SerializeField] private CellView      _cellPrefab;

    [Zenjex] private IGridInventoryService _inventoryService;
    [Zenjex] private BagConfig             _bagConfig;

    private readonly Dictionary<Vector2Int, CellView> _cellViews = new();
    private CompositeDisposable _disposables;

    protected override void OnAwake()
    {
      _disposables = new CompositeDisposable();
      SpawnGrid();
      SubscribeToInventory();
    }

    private void OnDestroy()
    {
      _disposables?.Dispose();
    }

    // ─── Rebuild (runtime config hot-swap) ───────────────────────────────────

    /// <summary>
    /// Пересоздаёт сетку по новому конфигу.
    /// Вызывать после GridInventory.UpdateActiveCells() если форма сумки изменилась в рантайме.
    /// </summary>
    public void Rebuild()
    {
      foreach (Transform child in _gridRoot)
        Destroy(child.gameObject);

      _cellViews.Clear();
      SpawnGrid();
      RefreshAll();
    }

    // ─── Grid spawn ───────────────────────────────────────────────────────────

    private void SpawnGrid()
    {
      var activeCells = _bagConfig.GetActiveCellsSet();
      float cellSize  = _bagConfig.CellSize;
      float spacing   = _bagConfig.CellSpacing;
      float step      = cellSize + spacing;

      // Размер корневого RectTransform подгоняем под сетку
      var gridSize = _bagConfig.GridSize;
      _gridRoot.sizeDelta = new Vector2(
        gridSize.x * step - spacing,
        gridSize.y * step - spacing
      );

      for (int x = 0; x < gridSize.x; x++)
      for (int y = 0; y < gridSize.y; y++)
      {
        var coord = new Vector2Int(x, y);
        var cell  = Instantiate(_cellPrefab, _gridRoot);

        // Ручное позиционирование (top-left origin)
        var rt = cell.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot     = new Vector2(0f, 1f);
        rt.sizeDelta = Vector2.one * cellSize;
        rt.anchoredPosition = new Vector2(
           x * step,
          -y * step
        );

        bool isActive = activeCells.Contains(coord);
        cell.Initialize(coord, isActive, HighlightItem);

        _cellViews[coord] = cell;
      }
    }

    // ─── Subscriptions ────────────────────────────────────────────────────────

    private void SubscribeToInventory()
    {
      _inventoryService.OnItemPlaced
        .Subscribe(_ => RefreshAll())
        .AddTo(_disposables);

      _inventoryService.OnItemRemoved
        .Subscribe(_ => RefreshAll())
        .AddTo(_disposables);

      _inventoryService.OnItemsMerged
        .Subscribe(result => OnMerge(result))
        .AddTo(_disposables);
    }

    // ─── Refresh ──────────────────────────────────────────────────────────────

    private void RefreshAll()
    {
      foreach (var cv in _cellViews.Values)
        cv.RefreshView();
    }

    private void OnMerge(MergeResult result)
    {
      RefreshAll();
      PlayMergeEffect(result.Result.Origin);
    }

    private void PlayMergeEffect(Vector2Int origin)
    {
      if (!_cellViews.TryGetValue(origin, out var cell)) return;

      // Вспышка + масштаб
      LeanTween
        .scale(cell.gameObject, Vector3.one * 1.25f, 0.1f)
        .setEaseOutQuad()
        .setOnComplete(() =>
          LeanTween.scale(cell.gameObject, Vector3.one, 0.15f).setEaseInBack());
    }

    // ─── Highlight API (вызывается из CellView) ───────────────────────────────

    /// <summary>
    /// Подсвечивает все ячейки формы предмета при размещении по origin.
    /// origin уже вычислен в CellView с учётом DragOffset.
    /// </summary>
    public void HighlightItem(ItemConfig config, Vector2Int origin, HighlightState state)
    {
      foreach (var offset in config.Shape)
      {
        var targetCell = origin + offset;
        if (_cellViews.TryGetValue(targetCell, out var cv))
          cv.SetHighlight(state);
      }
    }
  }
}
