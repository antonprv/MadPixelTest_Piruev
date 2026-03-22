// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Cysharp.Threading.Tasks;

namespace Code.UI.Factory
{
  /// <summary>
  /// Responsible for creating and wiring all gameplay UI.
  ///
  /// Loads BagCanvas from Addressables, then calls Construct() on each View
  /// with the appropriate ViewModel — fully bypassing Unity's Awake-based
  /// DI injection. This guarantees that domain services are already initialized
  /// before any View tries to read from them.
  /// </summary>
  public interface IUIFactory
  {
    UniTask CreateGameplayUIAsync();
  }
}
