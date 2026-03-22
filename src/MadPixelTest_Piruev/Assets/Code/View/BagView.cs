// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

using Code.Infrastructure.AssetManagement;
using Code.Model.Core;
using Code.ViewModel.Bag;

using Cysharp.Threading.Tasks;

using R3;

using UnityEngine;
using UnityEngine.UI;

namespace Code.View
{
  /// <summary>
  /// MVVM View — the inventory bag grid.
  ///
  /// Item icon management:
  ///   Each placed item gets its own Image GameObject that spans its full
  ///   bounding box. The image is a child of _iconsRoot (a sibling of _gridRoot,
  ///   placed above it in the hierarchy so icons render on top of cells).
  ///
  ///   Size    = boundsSize * step - spacing
  ///   Position = origin cell's anchoredPosition (top-left anchored)
  ///
  /// This ensures multi-cell items (L-shapes, T-shapes, etc.) display their
  /// icon at the correct size across all occupied cells.
  /// </summary>
  public class BagView : MonoBehaviour
  {
    [SerializeField] private RectTransform _gridRoot;
    [SerializeField] private RectTransform _iconsRoot;   // sibling above _gridRoot
    [SerializeField] private CellView      _cellPrefab;

    private IBagViewModel _bagViewModel;
    private IAssetLoader  _assetLoader;

    // Tracks icon GameObjects by item instance so we can destroy them on removal
    private readonly Dictionary<InventoryItem, GameObject> _itemIcons = new();
    private readonly Dictionary<Vector2Int, CellView>      _cellViews = new();

    private CompositeDisposable _disposables;

    private float _cellSize;
    private float _step;
    private float _spacing;

    /// <summary>Computed cell size + spacing — used by DragIconView to size the drag icon.</summary>
    public float CellStep    => _step;
    public float CellSpacing => _spacing;

    /// <summary>Called by UIFactory after domain services are initialized.</summary>
    public void Construct(IBagViewModel bagViewModel, IAssetLoader assetLoader)
    {
      _bagViewModel = bagViewModel;
      _assetLoader  = assetLoader;
      _disposables  = new CompositeDisposable();

      SpawnGrid();
      SpawnIconsForExistingItems();
      BindEvents();
    }

    private void OnDestroy() => _disposables?.Dispose();

    #region Grid spawn

    private void SpawnGrid()
    {
      var gridSize = _bagViewModel.GridSize;
      _spacing  = _bagViewModel.CellSpacing;
      _cellSize = ComputeCellSize(gridSize, _spacing);
      _step     = _cellSize + _spacing;
      var active = _bagViewModel.ActiveCells;

      _gridRoot.sizeDelta = new Vector2(
        gridSize.x * _step - _spacing,
        gridSize.y * _step - _spacing);

      // _iconsRoot same size so icon positions align 1:1
      if (_iconsRoot != null)
        _iconsRoot.sizeDelta = _gridRoot.sizeDelta;

      for (int x = 0; x < gridSize.x; x++)
        for (int y = 0; y < gridSize.y; y++)
        {
          var coord    = new Vector2Int(x, y);
          var cell     = Instantiate(_cellPrefab, _gridRoot);
          var isActive = active.Contains(coord);

          var rt = cell.GetComponent<RectTransform>();
          rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
          rt.pivot      = new Vector2(0f, 1f);
          rt.sizeDelta  = Vector2.one * _cellSize;
          rt.anchoredPosition = new Vector2(x * _step, -y * _step);

          cell.SetViewModel(_bagViewModel.GetCellViewModel(coord), coord, isActive);
          _cellViews[coord] = cell;
        }
    }

    #endregion

    #region Icon management

    /// <summary>
    /// Spawn icons for items that were placed before the view was created
    /// (startup items placed in GameLoopState.Enter before UIFactory runs).
    /// </summary>
    private void SpawnIconsForExistingItems()
    {
      foreach (var item in _bagViewModel.GetAllItems())
        SpawnIconAsync(item).Forget();
    }

    private void BindEvents()
    {
      _bagViewModel.OnItemPlaced
        .Subscribe(item => SpawnIconAsync(item).Forget())
        .AddTo(_disposables);

      _bagViewModel.OnItemRemoved
        .Subscribe(DestroyIcon)
        .AddTo(_disposables);

      _bagViewModel.OnMergeAnimation
        .Subscribe(PlayMergeEffect)
        .AddTo(_disposables);
    }

    private async UniTaskVoid SpawnIconAsync(InventoryItem item)
    {
      if (item.Config.Icon == null) return;

      var sprite = await _assetLoader.LoadAsync<Sprite>(item.Config.Icon);
      if (sprite == null) return;

      // Item may have been removed while we were loading
      if (_itemIcons.ContainsKey(item)) return;

      var root = _iconsRoot != null ? _iconsRoot : _gridRoot;

      var go = new GameObject($"Icon_{item.Config.ItemId}", typeof(RectTransform), typeof(Image));
      go.transform.SetParent(root, worldPositionStays: false);

      var img = go.GetComponent<Image>();
      img.sprite       = sprite;
      img.raycastTarget = false;
      img.preserveAspect = true;

      var bounds = item.Config.GetBoundsSize();
      var rt     = go.GetComponent<RectTransform>();
      rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
      rt.pivot      = new Vector2(0f, 1f);

      // Size = bounding box in grid cells, converted to pixel size
      rt.sizeDelta = new Vector2(
        bounds.x * _step - _spacing,
        bounds.y * _step - _spacing);

      // Position = origin cell position
      rt.anchoredPosition = new Vector2(
         item.Origin.x * _step,
        -item.Origin.y * _step);

      _itemIcons[item] = go;

      // Spawn animation — matches the cell place animation (scale pop with overshoot)
      go.transform.localScale = Vector3.one * 0.7f;
      LeanTween.scale(go, Vector3.one, 0.2f).setEaseOutBack();
    }

    private void DestroyIcon(InventoryItem item)
    {
      if (!_itemIcons.TryGetValue(item, out var go)) return;
      _itemIcons.Remove(item);
      if (go != null) Destroy(go);
    }

    #endregion

    #region Merge animation

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

    #region Adaptive cell size

    private float ComputeCellSize(Vector2Int gridSize, float spacing)
    {
      UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(_gridRoot);
      var size = _gridRoot.rect.size;

      if (size.x <= 1f || size.y <= 1f)
        size = EstimateContainerSize(gridSize, spacing);

      float fromWidth  = (size.x - spacing * (gridSize.x - 1)) / gridSize.x;
      float fromHeight = (size.y - spacing * (gridSize.y - 1)) / gridSize.y;
      return Mathf.Max(1f, Mathf.Min(fromWidth, fromHeight));
    }

    private Vector2 EstimateContainerSize(Vector2Int gridSize, float spacing)
    {
      var canvas = GetComponentInParent<Canvas>();
      var cr     = canvas != null
        ? canvas.GetComponent<RectTransform>().rect
        : new Rect(0, 0, Screen.width, Screen.height);
      return new Vector2(cr.width * 0.9f, cr.height * 0.6f);
    }

    #endregion
  }
}
