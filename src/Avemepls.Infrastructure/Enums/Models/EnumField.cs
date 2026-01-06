namespace Avemepls.Infrastructure.Enums.Models;

public class EnumField
{
    public string Key { get; }

    /// <summary>
    /// Название элемента
    /// </summary>
    public string Value { get; }

    public EnumField(string key, string value)
    {
        Key = key;
        Value = value;
    }
}