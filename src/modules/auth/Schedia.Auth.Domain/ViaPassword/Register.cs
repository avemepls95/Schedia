using System.Security.Cryptography;

using Avemepls.Auth.Bearer;
using Avemepls.Auth.Bearer.Abstractions;
using Avemepls.Core.DataAccess.Behaviors;
using Avemepls.Core.DataAccess.Extensions;
using Avemepls.Core.Extensions;
using Avemepls.Core.Localization;
using Avemepls.Domain.Validators;
using Avemepls.Identity.DataAccess;
using Avemepls.Identity.DataAccess.Models;
using Avemepls.Infrastructure.Email;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Schedia.Auth.Domain.Services;
using Schedia.Auth.Domain.Validators;

namespace Schedia.Auth.Domain.ViaPassword;

[Transaction]
public static class Register
{
    public class Command : IRequest<TokenInformation>
    {
        public string? Username { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }
    }

    internal sealed class Handler(
        IEmailService emailService,
        IdentityDbContext dbContext,
        IConfiguration configuration,
        ITokenGenerator tokenGenerator) : IRequestHandler<Command, TokenInformation>
    {
        public async Task<TokenInformation> Handle(Command command, CancellationToken cancellationToken)
        {
            var confirmationToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

            var username = command.Username.IsNullOrWhiteSpace()
                ? command.Email
                : command.Username!;

            var user = new User
            {
                Username = username,
                Email = command.Email,
                PasswordHash = PasswordHasher.HashPassword(command.Password),
                IsActive = true,
                EmailConfirmed = false,
                EmailConfirmationToken = confirmationToken,
                EmailConfirmationTokenExpiry = DateTimeOffset.UtcNow.AddDays(7),
                DateCreated = DateTimeOffset.UtcNow
            };

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync(cancellationToken);

            // TODO: Base URL from Options
            var confirmationLink = $"{configuration["AppSettings:BaseUrl"]}/confirm-email?token={Uri.EscapeDataString(confirmationToken)}";

            // TODO: via INotificationHandler
            await emailService.SendEmailConfirmationAsync(command.Email, username, confirmationLink, cancellationToken);

            var token = tokenGenerator.Create(user.Id);

            return token;
        }
    }

    public class Validator<TCommand> : ExtendedAbstractValidator<TCommand>
        where TCommand : Command
    {
        public Validator(IDbContextFactory<IdentityDbContext> dbContextFactory)
        {
            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .EmailAddress()
                .MustAsync(async (value, cancellationToken) =>
                {
                    await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
                    return !await dbContext.Users
                        .Available()
                        .AnyAsync(u => u.Email == value, cancellationToken);
                })
                .WithMessage("Пользователь с таким email уже существует");

            RuleFor(x => x.Password)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .SetValidator(new PasswordValidator());

            RuleFor(x => x.Username)
                .MustAsync(async (value, cancellationToken) =>
                {
                    await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
                    return !await dbContext.Users
                        .Available()
                        .AnyAsync(u => u.Username == value, cancellationToken);
                })
                .WithMessage("Выбранное имя занято")
                .When(x => !x.Username.IsNullOrWhiteSpace());
        }
    }
}