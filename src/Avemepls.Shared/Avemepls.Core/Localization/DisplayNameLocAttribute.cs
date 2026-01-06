using System.ComponentModel;
using System.Globalization;

namespace Avemepls.Core.Localization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Field)]
public class DisplayNameLocAttribute : DisplayNameAttribute
{
    private readonly Type? _resourceType;

    /// <summary>
    /// Specifies the display name for a property or event
    /// </summary>
    /// <param name="displayName">Display name</param>
    /// <param name="resourceType">Type to provide strings for</param>
    public DisplayNameLocAttribute(string displayName, Type? resourceType = null) : base(displayName)
    {
        _resourceType = resourceType;
    }

    public override string DisplayName
    {
        get
        {
            var locName = ILocalizationContext.GetString(DisplayNameValue, _resourceType ?? typeof(string));

            return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName != "ru" && locName == DisplayNameValue
                ? $"[{locName}]"
                : locName;
        }
    }
}