namespace Avemepls.Infrastructure.Enums.Models;

public class EnumField(string key, string value)
{
    public string Key { get; } = key;

    /// <summary>
    /// Название элемента
    /// </summary>
    public string Value { get; } = value;
}