// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

using Code.Infrastructure.AssetManagement;
using Code.Infrastructure.Services.StaticData.Interfaces;
using Code.Model.Core;
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
    Vector2Int          GridSize    { get; }
    float               CellSize    { get; }
    float               CellSpacing { get; }
    HashSet<Vector2Int> ActiveCells { get; }

    ICellViewModel GetCellViewModel(Vector2Int coord);

    /// <summary>Fires when a merge animation should play at the given origin cell.</summary>
    Observable<Vector2Int> OnMergeAnimation { get; }

    /// <summary>Fires when an item is placed — BagView spawns its icon.</summary>
    Observable<InventoryItem> OnItemPlaced { get; }

    /// <summary>Fires when an item is removed — BagView destroys its icon.</summary>
    Observable<InventoryItem> OnItemRemoved { get; }

    /// <summary>
    /// All items currently on the grid at construction time.
    /// BagView uses this to spawn icons for items that were already placed
    /// before the view was created (startup items).
    /// </summary>
    IReadOnlyList<InventoryItem> GetAllItems();
    #endregion
  }

  /// <summary>
  /// MVVM ViewModel for the entire bag grid.
  ///
  /// Icon management moved to BagView:
  ///   BagView listens to OnItemPlaced / OnItemRemoved and spawns/destroys
  ///   a full-bounding-box Image for each item. This makes icons correctly
  ///   cover multi-cell shapes instead of being clipped to a single cell.
  ///
  ///   CellViewModel no longer owns Icon / IconVisible properties.
  /// </summary>
  public class BagViewModel : IBagViewModel
  {
    private readonly Dictionary<Vector2Int, CellViewModel> _cellViewModels = new();
    private readonly Subject<Vector2Int>    _onMergeAnimation = new();
    private readonly CompositeDisposable    _disposables      = new();

    public Vector2Int          GridSize    { get; }
    public float               CellSize    { get; }
    public float               CellSpacing { get; }
    public HashSet<Vector2Int> ActiveCells { get; }

    public Observable<Vector2Int>    OnMergeAnimation => _onMergeAnimation;
    public Observable<InventoryItem> OnItemPlaced     { get; }
    public Observable<InventoryItem> OnItemRemoved    { get; }

    private readonly IBagPresenter _bagPresenter;

    private static readonly Color DefaultEmptyColor = new(0.15f, 0.15f, 0.15f, 0.6f);

    public BagViewModel(
      IBagConfigSubservice bagConfig,
      IBagPresenter        bagPresenter,
      IDragDropPresenter   dragDropPresenter,
      IAssetLoader         assetLoader)
    {
      _bagPresenter = bagPresenter;

      GridSize    = bagConfig.GridSize;
      CellSize    = bagConfig.CellSize;
      CellSpacing = bagConfig.CellSpacing;
      ActiveCells = bagConfig.GetActiveCellsSet();

      // Forward presenter events directly — BagView subscribes to these
      OnItemPlaced  = bagPresenter.OnItemPlaced;
      OnItemRemoved = bagPresenter.OnItemRemoved;

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

    public IReadOnlyList<InventoryItem> GetAllItems() =>
      _bagPresenter.GetAllItems();

    public void Dispose()
    {
      foreach (var vm in _cellViewModels.Values)
        vm.Dispose();
      _cellViewModels.Clear();
      _disposables.Dispose();
    }
  }
}
