using AntDesign;

using Avemepls.Core.Extensions;

namespace Avemepls.Blazor.Components;

public class EnumSelectExtended<TEnum> : EnumSelect<TEnum>
{
    protected override string GetLabel(TEnum item)
    {
        var value = (item as Enum)?.GetEnumFieldDisplayName();

        if (value != null)
        {
            return value;
        }

        return base.GetLabel(item);
    }
}