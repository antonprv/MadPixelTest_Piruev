// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Code.View.Interfaces;

namespace Code.Infrastructure.Services
{
  /// <summary>
  /// Minimal service locator for ISlotScreenPositionProvider.
  ///
  /// Used because BottomSlotsView is created after the DI container is built
  /// and cannot be registered as a binding at install time.
  /// DragDropPresenter reads from this locator instead of receiving the
  /// provider via constructor injection.
  /// </summary>
  public static class SlotPositionProviderLocator
  {
    public static ISlotScreenPositionProvider Instance { get; private set; }

    public static void Register(ISlotScreenPositionProvider provider) =>
      Instance = provider;

    public static void Unregister() =>
      Instance = null;
  }
}
