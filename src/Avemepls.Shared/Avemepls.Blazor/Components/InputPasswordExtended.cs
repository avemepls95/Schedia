using AntDesign;

namespace Avemepls.Blazor.Components;

public class InputPasswordExtended : InputPassword
{
    public InputPasswordExtended()
    {
        DebounceMilliseconds = BlazorGlobalConfiguration.InputDebounceMilliseconds;
    }
}