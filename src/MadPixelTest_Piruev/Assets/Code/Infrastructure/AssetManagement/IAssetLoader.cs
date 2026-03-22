using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace BagFight.Infrastructure.AssetManagement
{
  public interface IAssetLoader
  {
    /// <summary>Инициализация Addressables. Вызывается один раз в BootstrapState.</summary>
    UniTask InitializeAsync();

    /// <summary>Загружает ассет по AssetReference. Кэширует хэндл — повторный вызов мгновенный.</summary>
    UniTask<T> LoadAsync<T>(AssetReference reference) where T : Object;

    /// <summary>Загружает ассет по строковому адресу. Кэширует хэндл.</summary>
    UniTask<T> LoadAsync<T>(string address) where T : Object;

    /// <summary>Освобождает все загруженные хэндлы. Вызывается при смене уровня.</summary>
    void Cleanup();
  }
}
