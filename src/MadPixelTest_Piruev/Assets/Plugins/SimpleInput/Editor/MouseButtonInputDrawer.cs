// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using UnityEditor;

namespace SimpleInputNamespace
{
  [CustomPropertyDrawer(typeof(SimpleInput.MouseButtonInput))]
  public class MouseButtonInputDrawer : BaseInputDrawer
  {
    public override string ValueToString(SerializedProperty valueProperty)
    {
      return valueProperty.boolValue.ToString();
    }
  }
}