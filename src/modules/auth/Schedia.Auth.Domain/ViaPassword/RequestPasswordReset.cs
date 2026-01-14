using System.Security.Cryptography;

using Avemepls.Core.DataAccess.Extensions;
using Avemepls.Domain.Validators;
using Avemepls.Identity.DataAccess;
using Avemepls.Infrastructure.Email;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;

namespace Schedia.Auth.Domain.ViaPassword;

public static class RequestPasswordReset
{
    public class Command : IRequest
    {
        public string Email { get; set; }
    }

    internal sealed class Handler(
        IEmailService emailService,
        IDbContextFactory<IdentityDbContext> dbContextFactory,
        IConfiguration configuration) : IRequestHandler<Command>
    {
        public async Task Handle(Command command, CancellationToken cancellationToken)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var user = await dbContext.Users.AsTracking()
                .Available()
                .FirstAsync(u => u.Email == command.Email, cancellationToken);

            var resetToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

            user.PasswordResetToken = resetToken;
            user.PasswordResetTokenExpiry = DateTimeOffset.UtcNow.AddHours(1);
            user.DateUpdated = DateTimeOffset.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);

            // TODO: Base URL from Options
            var resetLink = $"{configuration["AppSettings:BaseUrl"]}/reset-password?token={Uri.EscapeDataString(resetToken)}";

            // TODO: via INotificationHandler
            await emailService.SendPasswordResetAsync(command.Email, user.Username, resetLink, cancellationToken);
        }
    }

    public class Validator : ExtendedAbstractValidator<Command>
    {
        public Validator(IDbContextFactory<IdentityDbContext> dbContextFactory, IStringLocalizer<Validator> loc)
        {
            RuleFor(r => r.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .MustAsync(async (value, cancellationToken) =>
                {
                    await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
                    return await dbContext.Users
                        .Available()
                        .AnyAsync(u => u.Email == value, cancellationToken);
                })
                .WithMessage(loc["Пользователь с таким email'ом не найден"]);
        }
    }
}