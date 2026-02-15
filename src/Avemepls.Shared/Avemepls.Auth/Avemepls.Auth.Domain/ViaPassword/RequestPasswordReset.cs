using System.Security.Cryptography;

using Avemepls.Auth.Abstractions;
using Avemepls.Core.DataAccess.Extensions;
using Avemepls.Core.Models;
using Avemepls.Domain.Exceptions;
using Avemepls.Domain.Validators;
using Avemepls.Identity.DataAccess;
using Avemepls.Identity.DataAccess.Models;
using Avemepls.Infrastructure.DateTime;
using Avemepls.Infrastructure.Email;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        AppOptions appOptions,
        AuthOptions authOptions) : IRequestHandler<Command>
    {
        public async Task Handle(Command command, CancellationToken cancellationToken)
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var user = await dbContext.Users.AsTracking()
                .Available()
                .FirstAsync(u => u.Email == command.Email, cancellationToken);

            if (user is null)
            {
                throw new ObjectNotFoundException(typeof(User), $"Email {command.Email} is invalid");
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
                .WithMessage(loc["Пользователь с указанным email'ом не найден"]);
        }
    }
}