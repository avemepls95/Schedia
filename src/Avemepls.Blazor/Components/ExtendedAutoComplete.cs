using AntDesign;

namespace Avemepls.Blazor.Components;

public class ExtendedAutoComplete<TOption> : AutoComplete<TOption>
{
    public ExtendedAutoComplete()
    {
        DebounceMilliseconds = BlazorGlobalConfiguration.InputDebounceMilliseconds;
    }
}