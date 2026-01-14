using Avemepls.Core.DataAccess.Behaviors;
using Avemepls.Core.DataAccess.Extensions;
using Avemepls.Identity.DataAccess;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Schedia.Identity.Domain.User;

[Transaction]
public static class ConfirmEmail
{
    public class Command : IRequest
    {
        public string Token { get; set; }
    }

    internal sealed class Handler(IdentityDbContext dbContext) : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await dbContext.Users.AsTracking()
                .Available()
                .FirstAsync(u => u.EmailConfirmationToken == request.Token, cancellationToken);

            user.EmailConfirmed = true;
            user.EmailConfirmationTokenExpiry = null;

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    internal sealed class Validator : AbstractValidator<Command>
    {
        public Validator(IDbContextFactory<IdentityDbContext> dbContextFactory, IStringLocalizer<Validator> loc)
        {
            RuleFor(u => u.Token)
                .NotEmpty()
                .CustomAsync(async (value, validationContext, cancellationToken) =>
                {
                    await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
                    var user = await dbContext.Users
                        .Available()
                        .FirstOrDefaultAsync(u => u.EmailConfirmationToken == value, cancellationToken);

                    if (user == null)
                    {
                        validationContext.AddFailure(loc["Не удалось найти пользователя. Пройдите процедуру подтверждения почты повторно"]);
                        return;
                    }

                    if (user.EmailConfirmed)
                    {
                        validationContext.AddFailure(loc["Почта уже подтверждена"]);
                        return;
                    }

                    if (user.EmailConfirmationTokenExpiry < DateTimeOffset.UtcNow)
                    {
                        validationContext.AddFailure(loc["Срок действия ссылки истек. Пройдите процедуру подтверждения почты повторно"]);
                    }
                });
        }
    }
}