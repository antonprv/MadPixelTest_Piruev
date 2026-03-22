// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System;
using System.Diagnostics;
using System.Reflection;

using ULog = UnityEngine.Debug;
using ULogType = UnityEngine.LogType;

namespace Code.Common.Extensions.Logging
{
  public class GameLogger : IGameLog
  {
    public void Log(string message)
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
      StackFrame frame = new(1);
      MethodBase callingMethod = frame.GetMethod();
      Type callerType = callingMethod?.DeclaringType;

      if (callerType != null && callingMethod != null)
      {
        ULog.Log(
          $"Log [{callerType.Name}.{callingMethod.Name}] {message}");
      }
      else
      {
#if UNITY_EDITOR
        ULog.LogWarning(
          $"{nameof(IGameLog)}: Unable to determine the caller's information for logging.");
#elif DEVELOPMENT_BUILD
        ULog.Log(message);
#endif
      }
#endif
    }

    public void Log(ULogType logType, string message)
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
      StackFrame frame = new(1);
      MethodBase callingMethod = frame.GetMethod();
      Type callerType = callingMethod?.DeclaringType;

      if (callerType != null && callingMethod != null)
      {
        ULog.unityLogger.Log(
          logType, $"Log [{callerType.Name}.{callingMethod.Name}] {message}");
      }
      else
      {
#if UNITY_EDITOR
        ULog.LogWarning(
          $"{nameof(IGameLog)}: Unable to determine the caller's information for logging.");
#elif DEVELOPMENT_BUILD
        ULog.Log(message);
#endif
      }
#endif
      }

    public void LogValue<TProperty, TValue>(TProperty property, TValue value)
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
      Log($"Set {nameof(property)} to {value}");
#endif
    }
  }
}
