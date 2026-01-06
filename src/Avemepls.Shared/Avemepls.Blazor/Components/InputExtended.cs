using AntDesign;

namespace Avemepls.Blazor.Components;

public class InputExtended<TValue> : Input<TValue>
{
    public InputExtended()
    {
        DebounceMilliseconds = BlazorGlobalConfiguration.InputDebounceMilliseconds;
    }
}