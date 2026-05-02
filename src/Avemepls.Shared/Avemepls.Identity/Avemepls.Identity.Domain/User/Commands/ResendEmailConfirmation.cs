using System.Security.Cryptography;

using Avemepls.Core.DataAccess.Extensions;
using Avemepls.Core.Extensions;
using Avemepls.Core.Models;
using Avemepls.Identity.Abstraction;
using Avemepls.Identity.DataAccess;
using Avemepls.Identity.DataAccess.Models;
using Avemepls.Infrastructure.DateTime;
using Avemepls.Infrastructure.Email;
using Avemepls.Security.Principal;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

using UserEntity = Avemepls.Identity.DataAccess.Models.User;

namespace Avemepls.Identity.Domain.User.Commands;

public static class ResendEmailConfirmation
{
    public class Command : IRequest
    {
    }

    internal sealed class Handler(
        IEmailService emailService,
        IdentityDbContext dbContext,
        IPrincipalAccessor principalAccessor,
        IdentityOptions options,
        AppOptions appOptions,
        ICurrentDateTimeProvider currentDateTimeProvider)
        : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = (await principalAccessor.GetPrincipal()).GetId()!.Value;
            var user = await dbContext.Users.Available().FirstAsync(u => u.Id == new Id<UserEntity>(userId), cancellationToken);

            if (user.Email.IsNullOrWhiteSpace())
            {
                throw new InvalidOperationException("Can not send confirmation mail because email not specified");
            }

            var confirmRecord = await dbContext.ConfirmEmailRecords.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
            if (confirmRecord is null)
            {
                confirmRecord = new ConfirmEmailRecord { UserId = userId };

                await dbContext.ConfirmEmailRecords.AddAsync(confirmRecord, cancellationToken);
            }

            confirmRecord.EmailConfirmationToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            confirmRecord.EmailConfirmationTokenExpiry = currentDateTimeProvider.UtcNow.AddHours(options.EmailConfirmationTokenLifeTimeInHours);

            await dbContext.SaveChangesAsync(cancellationToken);

            var confirmationLink = $"{appOptions.BaseUrl}/confirm-email?token={Uri.EscapeDataString(confirmRecord.EmailConfirmationToken)}";
            await emailService.SendEmailConfirmationAsync(user.Email, user.Username, confirmationLink, cancellationToken);
        }
    }

    internal sealed class Validator : AbstractValidator<Command>
    {
        public Validator(IDbContextFactory<IdentityDbContext> dbContextFactory, IPrincipalAccessor principalAccessor, IStringLocalizer<Validator> loc)
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
                        validationContext.AddFailure(loc["Что-то пошло не так. Пользователь не найден."]);
                        return;
                    }

                    if (user.EmailConfirmed)
                    {
                        validationContext.AddFailure(loc["Email уже подтвержден."]);
                    }
                });
        }
    }
}