using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Avemepls.Auth.Domain.Validators;

public class PasswordValidator : AbstractValidator<string>
{
    public PasswordValidator(IStringLocalizer<PasswordValidator> loc)
    {
        RuleFor(p => p)
            .MinimumLength(8).WithMessage(loc["Пароль должен содержать минимум 8 символов"])
            .MaximumLength(16).WithMessage(loc["Пароль должен содержать максимум 16 символов"])
            .Matches(@"[A-Z]+").WithMessage(loc["Пароль должен содержать хотя бы одну заглавную букву"])
            .Matches(@"[a-z]+").WithMessage(loc["Пароль должен содержать хотя бы одну строчную букву"])
            .Matches(@"[0-9]+").WithMessage(loc["Пароль должен содержать хотя бы одну цифру"]);
    }
}