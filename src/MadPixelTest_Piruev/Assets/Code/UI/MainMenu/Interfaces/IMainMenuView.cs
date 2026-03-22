// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System;

namespace Code.UI.MainMenu.Interfaces
{
  /// <summary>
  /// Implemented by the MonoBehaviour placed in the MainMenu scene.
  /// MainMenuState subscribes to these events after the scene loads.
  /// </summary>
  public interface IMainMenuView
  {
    event Action OnLevel1Clicked;
    event Action OnLevel2Clicked;
  }
}
