using Avemepls.Auth.Password.Models;
using Avemepls.Core.Extensions;
using Avemepls.Identity.DataAccess.Repositories;

using FluentValidation;

namespace Avemepls.Auth.Password.Validators;

internal sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator(IUserRepository userRepository)
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .CustomAsync(async (value, validationContext, cancellationToken) =>
            {
                if (await userRepository.EmailExistsAsync(value, cancellationToken))
                {
                    validationContext.AddFailure("Пользователь с таким email уже существует");
                }
            });

        RuleFor(x => x.Password).SetValidator(new PasswordValidator());

        RuleFor(x => x.Username)
            .CustomAsync(async (value, validationContext, cancellationToken) =>
            {
                if (await userRepository.EmailExistsAsync(value!, cancellationToken))
                {
                    validationContext.AddFailure("Пользователь с таким именем уже существует");
                }
            })
            .When(x => !x.Username.IsNullOrWhiteSpace());
    }
}