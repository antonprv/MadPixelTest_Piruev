// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using UnityEngine;

namespace Code.Gameplay.Utils
{
  public class DisableInGame : MonoBehaviour
  {
    private void Awake()
    {
      this.gameObject.SetActive(false);
    }
  }
}

