namespace Avemepls.Core.Mapping;

public class MapperConfig
{
    public bool ValidationEnabled { get; set; } = true;

    /// <summary>
    /// Convert empty or whitespace strings to null
    /// </summary>
    public bool EmptyStringToNull { get; set; } = true;
}