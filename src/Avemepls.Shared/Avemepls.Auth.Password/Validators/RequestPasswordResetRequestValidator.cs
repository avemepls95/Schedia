using Avemepls.Auth.Password.Models;
using Avemepls.Core.DataAccess.Extensions;
using Avemepls.Identity.DataAccess;

using FluentValidation;

using Microsoft.EntityFrameworkCore;

namespace Avemepls.Auth.Password.Validators;

public class RequestPasswordResetRequestValidator : AbstractValidator<RequestPasswordResetRequest>
{
    public RequestPasswordResetRequestValidator(IdentityDbContext dbContext)
    {
        RuleFor(r => r.Email)
            .NotEmpty().WithMessage("Email is required")
            .CustomAsync(async (value, validationContext, cancellationToken) =>
            {
                var userExists = await dbContext.Users
                    .Available()
                    .AnyAsync(u => u.Email == value, cancellationToken);

                if (!userExists)
                {
                    validationContext.AddFailure("Пользователь с таким email'ом не найден");
                }
            });
    }
}