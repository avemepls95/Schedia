using System.ComponentModel;
using System.Reflection;

using Avemepls.Core.Localization;

using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

namespace Schedia.Web.Shared.FluentValidationCustoms;

public static class ServiceCollectionScanExtensions
{
    public static IServiceCollection OverrideValidationMessages(this IServiceCollection _)
    {
        ValidatorOptions.Global.DisplayNameResolver = (_, member, _) => member != null
            ? member.GetCustomAttribute<DisplayNameLocAttribute>()?.DisplayName
              ?? member.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName
              ?? member.Name
            : null;

        ValidatorOptions.Global.LanguageManager = new CustomLanguageManager();

        return _;
    }
}