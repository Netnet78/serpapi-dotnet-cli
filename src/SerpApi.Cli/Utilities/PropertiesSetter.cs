using System.Reflection;

namespace SerpApi.Cli.Utilities;

public static class PropertiesSetter
{
  /// <summary>
  /// Checks all the properties of an object and execute callback each iteration.
  /// </summary>
  public static void IterateProperties<T>(this T @object, Action<T, PropertyInfo>? callback = null)
  {
    // Gets all the properties of OrganicResult 
    PropertyInfo[] properties = typeof(T).GetProperties();
    foreach (PropertyInfo property in properties)
    {
      callback?.Invoke(@object, property);
    }
  }
}