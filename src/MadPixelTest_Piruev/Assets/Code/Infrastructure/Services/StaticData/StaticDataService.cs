// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Infrastructure.Services.StaticData.Interfaces;

using Cysharp.Threading.Tasks;

namespace Code.Infrastructure.Services.StaticData
{
  /// <summary>
  /// Root static-data service.
  /// Aggregates subservices and exposes a single LoadAllAsync entry point
  /// so the state machine only needs to know about IStaticDataService.
  ///
  /// Loading order matters: BagConfig and ItemManifest are independent,
  /// so they are loaded in parallel via UniTask.WhenAll.
  /// </summary>
  public class StaticDataService : IStaticDataService
  {
    public IBagConfigSubservice BagConfig { get; }
    public IItemDataSubservice  ItemData  { get; }

    public StaticDataService(
      IBagConfigSubservice bagConfig,
      IItemDataSubservice  itemData)
    {
      BagConfig = bagConfig;
      ItemData  = itemData;
    }

    public async UniTask LoadAllAsync() =>
      await UniTask.WhenAll(
        BagConfig.LoadSelfAsync(),
        ItemData.LoadSelfAsync());
  }
}
