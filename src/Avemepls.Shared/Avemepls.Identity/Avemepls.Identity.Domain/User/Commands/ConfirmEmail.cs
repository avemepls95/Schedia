using System.Security.Cryptography;
using System.Text;

using Avemepls.Identity.Abstraction;
using Avemepls.Identity.DataAccess;
using Avemepls.Infrastructure.DateTime;
using Avemepls.Infrastructure.RateLimit;

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

    internal sealed class Handler(
        IdentityDbContext dbContext,
        IRateLimiter rateLimiter,
        IdentityOptions options,
        IStringLocalizer<Handler> loc) : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var rateLimitKey = $"email-confirm-verify:token:{HashToken(request.Token)}";
            var rateLimitResult = await rateLimiter.Acquire(
                rateLimitKey,
                options.EmailConfirmationVerifyRateLimit,
                cancellationToken);

            if (!rateLimitResult.Allowed)
            {
                var minutes = RateLimitFormatter.ToUserFriendlyMinutes(rateLimitResult.RetryAfter!.Value);
                var template = loc["Слишком много попыток подтверждения. Попробуйте через {0} мин."].Value;
                throw new ValidationException(string.Format(template, minutes));
            }

            var confirmEmailRecord = await dbContext.ConfirmEmailRecords.AsTracking()
                .Include(x => x.User)
                .FirstOrDefaultAsync(u =>
                    u.EmailConfirmationToken == request.Token && u.EmailConfirmationTokenExpiry >= DateTime.UtcNow,
                    cancellationToken)
                ?? throw new ValidationException(loc["Ссылка подтверждения некорректна либо время ее действия истекло"]);

            confirmEmailRecord.User!.EmailConfirmed = true;

            dbContext.ConfirmEmailRecords.Remove(confirmEmailRecord);

            await dbContext.SaveChangesAsync(cancellationToken);

            // Сделать так, чтобы после подтверждения мы логинили юзера, если он разлогинен
        }

        private static string HashToken(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token ?? string.Empty));
            return Convert.ToHexString(bytes);
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

                    if (confirmRecord.User!.EmailConfirmed)
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