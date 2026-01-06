using System.Text.RegularExpressions;

using FluentValidation;
using FluentValidation.Validators;

namespace Avemepls.Domain.Validators;

/// <summary>
/// Валидатор строки в формате Hex Color RGB
/// <typeparam name="T">Сущность</typeparam>
/// </summary>
#pragma warning disable SA1619
public partial class NullableHexRgbColorValidator<T> : PropertyValidator<T, string?>
#pragma warning restore SA1619
{
    private const string RegexPattern = "^#([a-fA-F0-9]{6})$";

    /// <summary>
    /// Initializes a new instance
    /// </summary>
    public NullableHexRgbColorValidator()
    {
    }

    protected override string GetDefaultMessageTemplate(string errorCode)
    {
        return "'{PropertyName}': '{AttemptedValue}' не соответствует формату Hex Color (RGB). Допустимы значения в формате: #Ab109F";
    }

    public override string Name { get; }

    /// <inheritdoc />
    public override bool IsValid(ValidationContext<T> context, string? hexColor)
    {
        if (hexColor == null)
        {
            return true;
        }

        var isValid = HexRegex().IsMatch(hexColor);

        if (!isValid)
        {
            context.MessageFormatter.AppendArgument("AttemptedValue", hexColor);
        }

        return isValid;
    }

    [GeneratedRegex(RegexPattern)]
    private static partial Regex HexRegex();
}