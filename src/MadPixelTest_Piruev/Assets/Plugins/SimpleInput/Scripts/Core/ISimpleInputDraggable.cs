// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using UnityEngine.EventSystems;

namespace SimpleInputNamespace
{
  public interface ISimpleInputDraggable
  {
    void OnPointerDown(PointerEventData eventData);
    void OnDrag(PointerEventData eventData);
    void OnPointerUp(PointerEventData eventData);
  }
}