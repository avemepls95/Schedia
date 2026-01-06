using System.Globalization;
using System.Reflection;

using Newtonsoft.Json;

namespace Avemepls.Blazor.Filters;

public static class ObjectKvpJsonPersister
{
    /// <summary>
    /// Convert object to key-value pair, where key is property name, value - json-serialized value
    /// </summary>
    /// <param name="prefix">Prefox for filter parameters in query</param>
    /// <param name="obj">Object to persist</param>
    /// <typeparam name="T">Type of object to persist</typeparam>
    /// <returns>IEnumerable of (string,string) pairs</returns>
    public static IEnumerable<KeyValuePair<string, string?>> Persist<T>(string? prefix, T obj)
    {
        foreach (var property in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            var key = prefix == null
                ? property.Name
                : prefix + "." + property.Name;

            var serializedValue = SerializeValue(property, obj);

            yield return new KeyValuePair<string, string?>(key, serializedValue);
        }
    }

    /// <summary>
    /// Restores state of object from key-value pairs of property name + property value serialized to JSON
    /// </summary>
    /// <param name="prefix">Prefix for parameters</param>
    /// <param name="obj">Instance to fill with restored data</param>
    /// <param name="keyValues">Data parts (key-value pairs)</param>
    /// <typeparam name="T">Type of instance to restore</typeparam>
    /// <returns>Same instance of object from input</returns>
    public static bool Restore<T>(string? prefix, T obj, IEnumerable<KeyValuePair<string, string?>> keyValues)
    {
        var fullPrefix = prefix + ".";
        bool anyRestored = false;

        foreach (var (name, value) in keyValues)
        {
            var propertyName = name;

            if (prefix == null || propertyName.StartsWith(fullPrefix))
            {
                propertyName = prefix == null
                    ? propertyName
                    : propertyName.Substring(fullPrefix.Length);

                var property = typeof(T).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

                if (property == null)
                    continue;

                property.SetValue(obj, DeserializeValue(property, value));
                anyRestored = true;
            }
        }

        return anyRestored;
    }

    private static string? SerializeValue<T>(PropertyInfo property, T obj)
    {
        var propertyValue = property.GetValue(obj);

        if (property.PropertyType == typeof(string) || property.PropertyType == typeof(char))
            return propertyValue?.ToString();

        if (property.PropertyType == typeof(DateTime) && propertyValue is not null)
            return ((DateTime)propertyValue).ToString("O");

        if (property.PropertyType == typeof(DateTime?))
            return ((DateTime?)propertyValue)?.ToString("O");

        if (property.PropertyType == typeof(DateOnly) && propertyValue is not null)
            return ((DateOnly)propertyValue).ToString("yyyy-MM-dd");

        if (property.PropertyType == typeof(DateOnly?))
            return ((DateOnly?)propertyValue)?.ToString("yyyy-MM-dd");

        return propertyValue == null
            ? null
            : JsonConvert.SerializeObject(propertyValue);
    }

    private static object? DeserializeValue(PropertyInfo property, string? value)
    {
        if (value is null)
            return null;

        if (property.PropertyType == typeof(string) || property.PropertyType == typeof(char))
            return value;

        if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
            return DateTime.ParseExact(value, "O", CultureInfo.InvariantCulture);

        if (property.PropertyType == typeof(DateOnly) || property.PropertyType == typeof(DateOnly?))
            return DateOnly.FromDateTime(DateTime.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture));

        return JsonConvert.DeserializeObject(value, property.PropertyType);
    }
}