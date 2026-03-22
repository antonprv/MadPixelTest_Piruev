// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using System;

using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Hud
{
  /// <summary>
  /// MonoBehaviour placed on the PUI_Hud prefab.
  ///
  /// Responsibilities:
  ///   - Owns the "Return to Menu" button
  ///   - Raises OnReturnClicked so GameLoopState can react
  ///
  /// Prefab setup:
  ///   Assign _returnButton in the Inspector.
  ///   The prefab is spawned by UIFactory as a child of UIRoot.
  /// </summary>
  public class HudView : MonoBehaviour
  {
    [SerializeField] private Button _returnButton;

    public event Action OnReturnClicked;

    private void Awake() =>
      _returnButton?.onClick.AddListener(() => OnReturnClicked?.Invoke());

    private void OnDestroy() =>
      _returnButton?.onClick.RemoveAllListeners();
  }
}
