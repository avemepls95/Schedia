using Avemepls.Identity.DataAccess;
using Avemepls.Infrastructure.DateTime;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Avemepls.Identity.Domain.User.Commands;

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
            var confirmEmailRecord = await dbContext.ConfirmEmailRecords.AsTracking()
                .Include(x => x.User)
                .FirstAsync(u => u.EmailConfirmationToken == request.Token, cancellationToken);

            confirmEmailRecord.User.EmailConfirmed = true;

            dbContext.ConfirmEmailRecords.Remove(confirmEmailRecord);

            await dbContext.SaveChangesAsync(cancellationToken);

            // Сделать так, чтобы после подтверждения мы логинили юзера, если он разлогинен
        }
    }

    internal sealed class Validator : AbstractValidator<Command>
    {
        public Validator(
            IDbContextFactory<IdentityDbContext> dbContextFactory,
            IStringLocalizer<Validator> loc,
            ICurrentDateTimeProvider currentDateTimeProvider)
        {
            RuleFor(u => u.Token)
                .NotEmpty()
                .CustomAsync(async (value, validationContext, cancellationToken) =>
                {
                    await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
                    var confirmRecord = await dbContext.ConfirmEmailRecords
                        .Include(x => x.User)
                        .FirstOrDefaultAsync(u => u.EmailConfirmationToken == value, cancellationToken);

                    if (confirmRecord == null)
                    {
                        validationContext.AddFailure(loc["Не удалось найти пользователя. Пройдите процедуру подтверждения почты повторно"]);
                        return;
                    }

                    if (confirmRecord.User.EmailConfirmed)
                    {
                        validationContext.AddFailure(loc["Почта уже подтверждена"]);
                        return;
                    }

                    if (confirmRecord.EmailConfirmationTokenExpiry < currentDateTimeProvider.UtcNow)
                    {
                        validationContext.AddFailure(loc["Срок действия ссылки истек. Пройдите процедуру подтверждения почты повторно"]);
                    }
                });
        }
    }
}