// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SimpleInputNamespace
{
  public class MouseButtonInputUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
  {
    public SimpleInput.MouseButtonInput mouseButton = new SimpleInput.MouseButtonInput();

    private void Awake()
    {
      Graphic graphic = GetComponent<Graphic>();
      if (graphic != null)
        graphic.raycastTarget = true;
    }

    private void OnEnable()
    {
      mouseButton.StartTracking();
    }

    private void OnDisable()
    {
      mouseButton.StopTracking();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
      mouseButton.value = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
      mouseButton.value = false;
    }
  }
}