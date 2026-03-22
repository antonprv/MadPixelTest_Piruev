// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System;

using Code.UI.MainMenu.Interfaces;

using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.MainMenu
{
  /// <summary>
  /// MonoBehaviour placed in the MainMenu scene.
  /// Wires Unity UI Button.onClick events to C# Action events
  /// that MainMenuState subscribes to.
  ///
  /// Setup in the scene:
  ///   Assign _level1Button, _level2Button, _quitButton via Inspector.
  ///   This component is found at runtime via Object.FindFirstObjectByType.
  /// </summary>
  public class MainMenuView : MonoBehaviour, IMainMenuView
  {
    [SerializeField] private Button _level1Button;
    [SerializeField] private Button _level2Button;

    public event Action OnLevel1Clicked;
    public event Action OnLevel2Clicked;

    private void Awake()
    {
      if (_level1Button != null)
        _level1Button.onClick.AddListener(() => OnLevel1Clicked?.Invoke());

      if (_level2Button != null)
        _level2Button.onClick.AddListener(() => OnLevel2Clicked?.Invoke());
    }

    private void OnDestroy()
    {
      if (_level1Button != null)
        _level1Button.onClick.RemoveAllListeners();

      if (_level2Button != null)
        _level2Button.onClick.RemoveAllListeners();
    }
  }
}
