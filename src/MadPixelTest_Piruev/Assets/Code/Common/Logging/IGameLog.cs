// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using UnityEngine;

namespace Code.Common.Extensions.Logging
{
  public interface IGameLog
  {
    void Log(string message);
    void Log(LogType logType, string message);
    void LogValue<TProperty, TValue>(TProperty property, TValue value);
  }
}
