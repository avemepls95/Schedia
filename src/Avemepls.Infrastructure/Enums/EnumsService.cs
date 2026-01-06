using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

using Avemepls.Core.Localization;
using Avemepls.Infrastructure.Enums.Models;

using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

using Namotion.Reflection;

namespace Avemepls.Infrastructure.Enums;

public class EnumsService
{
    private readonly Type[] _enumTypes;

    private readonly IStringLocalizerFactory _localizerFactory;
    private readonly EnumsServiceOptions _options;

    public EnumsService(IStringLocalizerFactory localizerFactory, IOptions<EnumsServiceOptions> options)
    {
        _localizerFactory = localizerFactory;
        _options = options.Value;

        _enumTypes = GetEnumTypes().ToArray();
    }

    public EnumModel[] GetAllEnums()
    {
        return GetEnums().ToArray();
    }

    private IEnumerable<EnumModel> GetEnums()
    {
        foreach (var enumType in _enumTypes)
        {
            yield return new EnumModel
            {
                Name = enumType.Name,
                Description = enumType.GetXmlDocsSummary(),
                Values = GetEnumValues(enumType).ToArray()
            };
        }
    }

    private IEnumerable<Type> GetEnumTypes()
    {
        return _options.Assemblies
            .Where(x => !_options.ExcludingAssemblies.Contains(x.GetName().Name!))
            .SelectMany(x => x.ExportedTypes)
            .Where(x => x.IsSubclassOf(typeof(Enum)));
    }

    private IEnumerable<EnumField> GetEnumValues(Type enumType)
    {
        var localizer = _localizerFactory.Create(enumType);

        foreach (var enumValue in Enum.GetValues(enumType).Cast<Enum>().Order())
        {
            var enumValueStr = enumValue.ToString();

            var displayLocAttribute = enumType.GetMember(enumValueStr)[0].GetCustomAttribute<DisplayNameLocAttribute>();

            if (displayLocAttribute is not null)
            {
                yield return new EnumField(enumValueStr, displayLocAttribute.DisplayName);
            }
            else
            {
                var displayAttribute = enumType.GetMember(enumValueStr)[0].GetCustomAttribute<DisplayAttribute>();

                var fieldDescription = displayAttribute is null
                    ? enumValueStr
                    : localizer[displayAttribute.Name!];

                fieldDescription = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName != "ru" &&
                                   displayAttribute?.Name == fieldDescription
                    ? $"[{fieldDescription}]"
                    : fieldDescription;

                yield return new EnumField(enumValueStr, fieldDescription);
            }
        }
    }
}