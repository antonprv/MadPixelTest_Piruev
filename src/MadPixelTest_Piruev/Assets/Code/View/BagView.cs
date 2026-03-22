// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

using Code.ViewModel.Bag;

using R3;

using UnityEngine;

using Zenjex.Extensions.Attribute;
using Zenjex.Extensions.Injector;

namespace Code.View
{
  /// <summary>
  /// MVVM View — the inventory bag grid.
  ///
  /// Responsibilities:
  ///   - Spawns CellView prefabs based on layout info from IBagViewModel
  ///   - Passes each CellView its ICellViewModel via SetViewModel()
  ///   - Subscribes to IBagViewModel.OnMergeAnimation → plays LeanTween effect
  ///
  /// Zero business logic. No Presenter references.
  /// </summary>
  public class BagView : ZenjexBehaviour
  {
    [SerializeField] private RectTransform _gridRoot;
    [SerializeField] private CellView _cellPrefab;

    [Zenjex] private IBagViewModel _bagViewModel;

    private readonly Dictionary<Vector2Int, CellView> _cellViews = new();
    private CompositeDisposable _disposables;

    protected override void OnAwake()
    {
      _disposables = new CompositeDisposable();
      SpawnGrid();
      BindMergeAnimation();
    }

    private void OnDestroy() => _disposables?.Dispose();

    #region Grid spawn

    private void SpawnGrid()
    {
      var gridSize = _bagViewModel.GridSize;
      var cellSize = _bagViewModel.CellSize;
      var spacing = _bagViewModel.CellSpacing;
      var step = cellSize + spacing;
      var active = _bagViewModel.ActiveCells;

      _gridRoot.sizeDelta = new Vector2(
        gridSize.x * step - spacing,
        gridSize.y * step - spacing);

      for (int x = 0; x < gridSize.x; x++)
        for (int y = 0; y < gridSize.y; y++)
        {
          var coord = new Vector2Int(x, y);
          var cell = Instantiate(_cellPrefab, _gridRoot);
          var isActive = active.Contains(coord);

          var rt = cell.GetComponent<RectTransform>();
          rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
          rt.pivot = new Vector2(0f, 1f);
          rt.sizeDelta = Vector2.one * cellSize;
          rt.anchoredPosition = new Vector2(x * step, -y * step);

          var cellViewModel = _bagViewModel.GetCellViewModel(coord);
          cell.SetViewModel(cellViewModel, coord, isActive);

          _cellViews[coord] = cell;
        }
    }
    #endregion

    #region Merge animation

    private void BindMergeAnimation()
    {
      _bagViewModel.OnMergeAnimation
        .Subscribe(PlayMergeEffect)
        .AddTo(_disposables);
    }

    private void PlayMergeEffect(Vector2Int origin)
    {
      if (!_cellViews.TryGetValue(origin, out var cell)) return;

      LeanTween
        .scale(cell.gameObject, Vector3.one * 1.25f, 0.1f)
        .setEaseOutQuad()
        .setOnComplete(() =>
          LeanTween.scale(cell.gameObject, Vector3.one, 0.15f).setEaseInBack());
    }
    #endregion
  }
}
