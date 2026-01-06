using Avemepls.Core.Extensions;

using FluentValidation;

using Microsoft.Extensions.Localization;

namespace Avemepls.Domain.Extensions;

/// <summary>
/// Методы расширения для FluentValidation
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Проверяет, что строка является корректным JSON
    /// </summary>
    public static IRuleBuilderOptions<T, string> IsJson<T>(this IRuleBuilder<T, string> ruleBuilder, IStringLocalizer loc)
    {
        return (IRuleBuilderOptions<T, string>)ruleBuilder.Custom((value, context) =>
        {
            if (!value.IsJson())
            {
                context.AddFailure(loc["Некорректный JSON формат настроек"]);
            }
        });
    }
}