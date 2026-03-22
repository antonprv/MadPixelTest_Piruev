// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using UnityEditor;

namespace SimpleInputNamespace
{
  [CustomPropertyDrawer(typeof(SimpleInput.AxisInput))]
  public class AxisInputDrawer : BaseInputDrawer
  {
    public override string ValueToString(SerializedProperty valueProperty)
    {
      return valueProperty.floatValue.ToString();
    }
  }
}