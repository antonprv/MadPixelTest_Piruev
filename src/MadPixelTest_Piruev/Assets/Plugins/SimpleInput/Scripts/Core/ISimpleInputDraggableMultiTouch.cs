// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System.Collections.Generic;

using UnityEngine.EventSystems;

namespace SimpleInputNamespace
{
  public interface ISimpleInputDraggableMultiTouch
  {
    int Priority { get; }

    bool OnUpdate(List<PointerEventData> mousePointers, List<PointerEventData> touchPointers, ISimpleInputDraggableMultiTouch activeListener);
  }
}