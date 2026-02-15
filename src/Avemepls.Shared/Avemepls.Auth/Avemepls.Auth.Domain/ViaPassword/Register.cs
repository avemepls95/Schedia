using Avemepls.Auth.Abstractions.Models;
using Avemepls.Auth.Bearer;
using Avemepls.Auth.Bearer.Abstractions;
using Avemepls.Auth.Domain.Services;
using Avemepls.Auth.Domain.Validators;
using Avemepls.Core.DataAccess.Behaviors;
using Avemepls.Core.DataAccess.Extensions;
using Avemepls.Core.Extensions;
using Avemepls.Domain.Validators;
using Avemepls.Identity.DataAccess;
using Avemepls.Identity.DataAccess.Models;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Avemepls.Auth.Domain.ViaPassword;

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
        IdentityDbContext dbContext,
        ITokenGenerator tokenGenerator,
        IPublisher publisher) : IRequestHandler<Command, TokenInformation>
    {
        public async Task<TokenInformation> Handle(Command command, CancellationToken cancellationToken)
        {
            var username = command.Username.IsNullOrWhiteSpace()
                ? command.Email
                : command.Username!;

            var user = new User
            {
                Username = username,
                Email = command.Email,
                PasswordHash = PasswordHasher.HashPassword(command.Password),
                IsActive = true,
                EmailConfirmed = false
            };

            await dbContext.Users.AddAsync(user, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            await publisher.Publish(new UserRegisteredViaPasswordNotification(user.Id), cancellationToken);

            var token = tokenGenerator.Create(user.Id);

            return token;
        }
    }

    public class Validator<TCommand> : ExtendedAbstractValidator<TCommand>
        where TCommand : Command
    {
        public Validator(
            IDbContextFactory<IdentityDbContext> dbContextFactory,
            IStringLocalizer<Validator<TCommand>> loc,
            IStringLocalizer<PasswordValidator> passwordLoc)
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
                .WithMessage(loc["Пользователь с таким email уже существует"]);

            RuleFor(x => x.Password)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .SetValidator(new PasswordValidator(passwordLoc));

            RuleFor(x => x.Username)
                .MustAsync(async (value, cancellationToken) =>
                {
                    await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
                    return !await dbContext.Users
                        .Available()
                        .AnyAsync(u => u.Username == value, cancellationToken);
                })
                .WithMessage(loc["Выбранное имя занято"])
                .When(x => !x.Username.IsNullOrWhiteSpace());
        }
    }
}