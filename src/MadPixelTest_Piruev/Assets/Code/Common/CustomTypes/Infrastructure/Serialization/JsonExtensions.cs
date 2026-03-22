// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using UnityEngine;

namespace Code.Common.CustomTypes.Infrastructure.Serialization
{
  /// <summary>
  /// JSON serialization extensions using Unity's JsonUtility
  /// </summary>
  public static class JsonExtensions
  {
    /// <summary>
    /// Serialize object to JSON string
    /// </summary>
    public static string ToJson<T>(this T obj, bool prettyPrint = false) =>
      JsonUtility.ToJson(obj, prettyPrint);

    /// <summary>
    /// Deserialize JSON string to object
    /// </summary>
    public static T FromJson<T>(this string json) =>
      JsonUtility.FromJson<T>(json);

    /// <summary>
    /// Try to deserialize, returns default if fails
    /// </summary>
    public static T TryFromJson<T>(this string json, T defaultValue = default)
    {
      if (string.IsNullOrEmpty(json))
        return defaultValue;

      try
      {
        return JsonUtility.FromJson<T>(json);
      }
      catch
      {
        return defaultValue;
      }
    }

    public static string ToSerialized(this object obj) => JsonUtility.ToJson(obj);

    public static T ToDeserialized<T>(this string json) => FromJson<T>(json);

    /// <summary>
    /// Overwrite existing object with JSON data
    /// </summary>
    public static void FromJsonOverwrite<T>(this string json, T target) =>
      JsonUtility.FromJsonOverwrite(json, target);
  }
}
