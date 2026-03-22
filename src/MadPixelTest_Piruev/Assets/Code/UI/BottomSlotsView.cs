// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

using Code.Data.StaticData;
using Code.Services.Interfaces;

using R3;

using UnityEngine;

using Zenjex.Extensions.Attribute;
using Zenjex.Extensions.Injector;

namespace Code.UI
{
  /// <summary>
  /// Spawns N BottomSlotView and syncs them with IBottomSlotsService.
  /// Horizontal row at the bottom of the screen.
  /// </summary>
  public class BottomSlotsView : ZenjexBehaviour
  {
    [SerializeField] private RectTransform _slotsRoot;
    [SerializeField] private BottomSlotView _slotPrefab;

    [Zenjex] private IBottomSlotsService _slotsService;
    [Zenjex] private BagConfig _bagConfig;

    private readonly List<BottomSlotView> _slots = new();
    private CompositeDisposable _disposables;

    protected override void OnAwake()
    {
      _disposables = new CompositeDisposable();
      SpawnSlots();
      SubscribeToSlots();
    }

    private void OnDestroy()
    {
      _disposables?.Dispose();
    }

    private void SpawnSlots()
    {
      for (int i = 0; i < _bagConfig.BottomSlotCount; i++)
      {
        var slot = Instantiate(_slotPrefab, _slotsRoot);
        slot.Initialize(i);
        _slots.Add(slot);
      }
    }

    private void SubscribeToSlots()
    {
      _slotsService.OnSlotChanged
        .Subscribe(index => _slots[index].RefreshView())
        .AddTo(_disposables);
    }
  }
}
