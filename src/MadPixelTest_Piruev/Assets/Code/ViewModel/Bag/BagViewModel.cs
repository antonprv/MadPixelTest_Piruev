// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

using Code.Infrastructure.AssetManagement;
using Code.Infrastructure.Services.StaticData.Interfaces;
using Code.Presenter.Bag;
using Code.Presenter.DragDrop;
using Code.ViewModel.Cell;

using R3;

using UnityEngine;

namespace Code.ViewModel.Bag
{
  public interface IBagViewModel
  {
    #region Layout info for BagView to build the grid
    Vector2Int GridSize    { get; }
    float      CellSize    { get; }
    float      CellSpacing { get; }
    HashSet<Vector2Int> ActiveCells { get; }

    /// <summary>Returns the CellViewModel for the given grid coordinate.</summary>
    ICellViewModel GetCellViewModel(Vector2Int coord);

    /// <summary>Fires when a merge animation should play at the given origin cell.</summary>
    Observable<Vector2Int> OnMergeAnimation { get; }
    #endregion
  }

  /// <summary>
  /// MVVM ViewModel for the entire bag grid.
  ///
  /// Responsibilities:
  ///   - Creates and owns a CellViewModel per active grid cell
  ///   - Subscribes to IBagPresenter.OnHighlightRequested → distributes highlight state
  ///     across the correct CellViewModels (multi-cell item highlight)
  ///   - Subscribes to IBagPresenter.OnItemsMerged → fires OnMergeAnimation
  ///     so BagView can play the LeanTween effect
  ///
  /// BagView never talks to a Presenter directly — it binds to this ViewModel.
  /// </summary>
  public class BagViewModel : IBagViewModel
  {
    private readonly Dictionary<Vector2Int, CellViewModel> _cellViewModels = new();
    private readonly Subject<Vector2Int> _onMergeAnimation = new();
    private readonly CompositeDisposable _disposables = new();

    public Vector2Int GridSize    { get; }
    public float      CellSize    { get; }
    public float      CellSpacing { get; }
    public HashSet<Vector2Int> ActiveCells { get; }

    public Observable<Vector2Int> OnMergeAnimation => _onMergeAnimation;

    private static readonly Color DefaultEmptyColor = new(0.15f, 0.15f, 0.15f, 0.6f);

    public BagViewModel(
      IBagConfigSubservice bagConfig,
      IBagPresenter        bagPresenter,
      IDragDropPresenter   dragDropPresenter,
      IAssetLoader         assetLoader)
    {
      GridSize    = bagConfig.GridSize;
      CellSize    = bagConfig.CellSize;
      CellSpacing = bagConfig.CellSpacing;
      ActiveCells = bagConfig.GetActiveCellsSet();

      for (int x = 0; x < GridSize.x; x++)
        for (int y = 0; y < GridSize.y; y++)
        {
          var coord = new Vector2Int(x, y);
          _cellViewModels[coord] = new CellViewModel(
            coord, bagPresenter, dragDropPresenter, assetLoader, DefaultEmptyColor);
        }

      bagPresenter.OnHighlightRequested
        .Subscribe(req =>
        {
          foreach (var offset in req.Config.Shape)
          {
            var targetCell = req.Origin + offset;
            if (_cellViewModels.TryGetValue(targetCell, out var vm))
              vm.SetHighlight(req.State);
          }
        })
        .AddTo(_disposables);

      bagPresenter.OnItemsMerged
        .Subscribe(result => _onMergeAnimation.OnNext(result.Result.Origin))
        .AddTo(_disposables);
    }

    public ICellViewModel GetCellViewModel(Vector2Int coord)
      => _cellViewModels.TryGetValue(coord, out var vm) ? vm : null;

    public void Dispose()
    {
      foreach (var vm in _cellViewModels.Values)
        vm.Dispose();
      _cellViewModels.Clear();
      _disposables.Dispose();
    }
  }
}
