namespace Avemepls.Auditor.Commands;

/// <summary>
/// Context to share data between command auditor steps
/// </summary>
public class CommandProcessingAuditorContext
{
    private readonly Dictionary<string, object?> _cache = new();

    /// <summary>
    /// Store data in context by key
    /// </summary>
    public void Add(string key, object? value)
    {
        _cache.Add(key, value);
    }

    /// <summary>
    /// Get data from context by key
    /// </summary>
    public T? Get<T>(string key)
    {
        if (_cache.TryGetValue(key, out var value))
        {
            if (value == null) return default;
            return (T?)value;
        }

        return default;
    }
}