using System.Security.Cryptography;

using Avemepls.Core.DataAccess.Extensions;
using Avemepls.Core.Models;
using Avemepls.Identity.DataAccess;
using Avemepls.Infrastructure.Email;
using Avemepls.Security.Principal;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using UserEntity = Avemepls.Identity.DataAccess.Models.User;

namespace Schedia.Identity.Domain.User;

public static class ResendEmailConfirmation
{
    public class Command : IRequest
    {
    }

    internal sealed class Handler(
        IEmailService emailService,
        IdentityDbContext dbContext,
        IConfiguration configuration,
        IPrincipalAccessor principalAccessor)
        : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = (await principalAccessor.GetPrincipal()).GetId()!.Value;
            var user = await dbContext.Users.AsTracking().Available().FirstAsync(u => u.Id == new Id<UserEntity>(userId), cancellationToken);

            var confirmationToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

            user.EmailConfirmationToken = confirmationToken;
            user.EmailConfirmationTokenExpiry = DateTimeOffset.UtcNow.AddDays(7);

            await dbContext.SaveChangesAsync(cancellationToken);

            var confirmationLink = $"{configuration["AppSettings:BaseUrl"]}/confirm-email?token={Uri.EscapeDataString(confirmationToken)}";
            await emailService.SendEmailConfirmationAsync(user.Email, user.Username, confirmationLink, cancellationToken);
        }
    }

    internal sealed class Validator : AbstractValidator<Command>
    {
        public Validator(IDbContextFactory<IdentityDbContext> dbContextFactory, IPrincipalAccessor principalAccessor)
        {
            RuleFor(u => u)
                .NotEmpty()
                .CustomAsync(async (_, validationContext, cancellationToken) =>
                {
                    await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
                    var userId = (await principalAccessor.GetPrincipal()).GetId()!.Value;
                    var user = await dbContext.Users.AsTracking().Available().FirstOrDefaultAsync(u => u.Id == new Id<UserEntity>(userId), cancellationToken);

                    if (user is null)
                    {
                        validationContext.AddFailure("Что-то пошло не так. Пользователь не найден.");
                        return;
                    }

                    if (user.EmailConfirmed)
                    {
                        validationContext.AddFailure("Email уже подтвержден.");
                    }
                });
        }
    }
}