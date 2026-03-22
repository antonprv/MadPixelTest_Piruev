// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

using Code.Data.StaticData;

using Cysharp.Threading.Tasks;

namespace Code.Infrastructure.Services.StaticData.Interfaces
{
  public interface IItemDataSubservice
  {
    /// <summary>All item configs from the manifest. Available after LoadSelfAsync.</summary>
    IReadOnlyList<ItemConfig> Items { get; }

    /// <summary>Returns config by ItemId, or null if not found.</summary>
    ItemConfig ForItem(string itemId);

    UniTask LoadSelfAsync();
  }
}
