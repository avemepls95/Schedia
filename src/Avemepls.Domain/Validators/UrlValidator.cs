using FluentValidation;
using FluentValidation.Validators;

namespace Avemepls.Domain.Validators;

/// <summary>
/// Валидатор для ссылок.
/// </summary>
public class UrlValidator<T, TProperty> : PropertyValidator<T, TProperty>
{
    public UrlValidator()
    {
    }

    protected override string GetDefaultMessageTemplate(string errorCode)
    {
        return "'{PropertyName}' должно быть URL.";
    }

    public override bool IsValid(ValidationContext<T> context, TProperty value)
    {
        var url = Convert.ToString(value);

        if (!Uri.TryCreate(url, UriKind.Absolute, out var outUri))
        {
            return false;
        }

        return outUri.Scheme == Uri.UriSchemeHttp || outUri.Scheme == Uri.UriSchemeHttps;
    }

    public override string Name => "UrlValidator";
}