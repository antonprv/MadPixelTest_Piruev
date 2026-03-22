// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Infrastructure.Services.StaticData.Interfaces;

using Cysharp.Threading.Tasks;

namespace Code.Infrastructure.Services.StaticData
{
  public interface IStaticDataService
  {
    IBagConfigSubservice    BagConfig { get; }
    IItemDataSubservice     ItemData  { get; }
    ILevelStaticDataService LevelData { get; }

    /// <summary>
    /// Loads all static data required before gameplay.
    /// Called once in PreloadAssetsState.
    /// </summary>
    UniTask LoadAllAsync();
  }
}
