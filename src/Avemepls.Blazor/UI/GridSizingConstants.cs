namespace Avemepls.Blazor.UI;

public static class GridSizingConstants
{
    public static readonly IReadOnlyDictionary<string, int> Gutter = new Dictionary<string, int>
    {
        ["xs"] = 8,
        ["sm"] = 16,
        ["md"] = 24,
        ["lg"] = 32,
        ["xl"] = 48,
        ["xxl"] = 64
    };
}