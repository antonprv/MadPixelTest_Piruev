// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System;

using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class FilteredEnumAttribute : PropertyAttribute
{
  public Enum[] ExcludedValues { get; }

  public FilteredEnumAttribute(params object[] excludedValues)
  {
    ExcludedValues = new Enum[excludedValues.Length];

    for (int i = 0; i < excludedValues.Length; i++)
      ExcludedValues[i] = (Enum)excludedValues[i];
  }
}
