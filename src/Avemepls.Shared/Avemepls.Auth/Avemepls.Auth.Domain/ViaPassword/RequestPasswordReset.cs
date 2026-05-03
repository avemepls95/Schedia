using System.Security.Cryptography;

using Avemepls.Auth.Abstractions;
using Avemepls.Core.DataAccess.Extensions;
using Avemepls.Core.Models;
using Avemepls.Domain.Validators;
using Avemepls.Identity.DataAccess;
using Avemepls.Identity.DataAccess.Models;
using Avemepls.Infrastructure.DateTime;
using Avemepls.Infrastructure.Email;
using Avemepls.Infrastructure.RateLimit;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

using Schedia.Core;

namespace Avemepls.Auth.Domain.ViaPassword;

public static class RequestPasswordReset
{
    public class Command : IRequest
    {
        public string Email { get; set; }
    }

    internal sealed class Handler(
        IEmailService emailService,
        IDbContextFactory<IdentityDbContext> dbContextFactory,
        ICurrentDateTimeProvider currentDateTimeProvider,
        IRateLimiter rateLimiter,
        IStringLocalizer<Handler> loc,
        AppOptions appOptions,
        AuthOptions authOptions) : IRequestHandler<Command>
    {
        public async Task Handle(Command command, CancellationToken cancellationToken)
        {
            var rateLimitKey = $"pwd-reset:email:{command.Email.Trim().ToLowerInvariant()}";
            var rateLimitResult = await rateLimiter.Acquire(
                rateLimitKey,
                authOptions.PasswordResetRateLimit,
                cancellationToken);

            if (!rateLimitResult.Allowed)
            {
                var minutes = RateLimitFormatter.ToUserFriendlyMinutes(rateLimitResult.RetryAfter!.Value);
                var template = loc["Слишком много запросов на сброс пароля. Попробуйте через {0} мин."].Value;
                throw new ValidationException(string.Format(template, minutes));
            }

            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var user = await dbContext.Users.AsTracking()
                .Available()
                .FirstOrDefaultAsync(u => u.Email == command.Email, cancellationToken);

            if (user is null)
            {
                return;
            }

            var resetToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

            await dbContext.RequestResetPasswordRecords.AddAsync(
                new RequestResetPasswordRecord
                {
                    UserId = user.Id,
                    Token = resetToken,
                    TokenExpiry = currentDateTimeProvider.UtcNow.AddHours(authOptions.ResetPasswordTokenLifeTimeInHours)
                },
                cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);

            var resetLink = $"{appOptions.BaseUrl}/reset-password?token={Uri.EscapeDataString(resetToken)}";

            await emailService.SendPasswordResetAsync(command.Email, user.Username, resetLink, cancellationToken);
        }
    }

    public class Validator : ExtendedAbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(r => r.Email)
                .NotEmpty()
                .EmailAddress();
        }
    }
}