using AntDesign;

namespace Avemepls.Blazor.Components;

public class SearchExtended : Search
{
    public SearchExtended()
    {
        DebounceMilliseconds = BlazorGlobalConfiguration.InputDebounceMilliseconds;
    }

    public bool GetIsFocused() => this.IsFocused;
}