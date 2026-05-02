using Avemepls.Auth.Abstractions.Models;
using Avemepls.Auth.Bearer;
using Avemepls.Auth.Bearer.Abstractions;
using Avemepls.Auth.Domain.Extensions;
using Avemepls.Core.DataAccess.Behaviors;
using Avemepls.Core.DataAccess.Extensions;
using Avemepls.Domain.Validators;
using Avemepls.Identity.DataAccess;
using Avemepls.Identity.DataAccess.Models;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avemepls.Auth.Domain.ViaGoogle;

[Transaction]
public static class LoginViaGoogle
{
    public class Command : IRequest<TokenInformation>
    {
        public required string GoogleId { get; set; }
        public required string Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }

    internal sealed class Handler(
        IdentityDbContext dbContext,
        ITokenGenerator tokenGenerator,
        IPublisher publisher) : IRequestHandler<Command, TokenInformation>
    {
        public async Task<TokenInformation> Handle(Command command, CancellationToken cancellationToken)
        {
            var (user, _) = await GetOrCreateUser(command, cancellationToken);

            var token = tokenGenerator.Create(
                new UserData<int>
                {
                    Id = user.Id,
                    UserName = user.Username,
                    FullName = user.GetFullName(),
                    Email = user.Email
                });

            await publisher.Publish(
                new UserLoginNotification
                {
                    Id = user.Id,
                    Date = DateTimeOffset.UtcNow
                },
                cancellationToken);

            return token;
        }

        private async Task<(User User, bool IsNewUser)> GetOrCreateUser(Command command, CancellationToken cancellationToken)
        {
            var user = await dbContext.Users
                .Available()
                .FirstOrDefaultAsync(u => u.GoogleId == command.GoogleId, cancellationToken);

            if (user is not null)
            {
                return (user, false);
            }

            user = await dbContext.Users
                .Available()
                .FirstOrDefaultAsync(u => u.Email == command.Email, cancellationToken);

            if (user is not null)
            {
                user.GoogleId = command.GoogleId;
                await dbContext.SaveChangesAsync(cancellationToken);
                return (user, false);
            }

            var newUser = new User
            {
                Username = command.Email,
                Email = command.Email,
                GoogleId = command.GoogleId,
                FirstName = command.FirstName,
                LastName = command.LastName,
                IsActive = true,
                EmailConfirmed = true
            };

            await dbContext.Users.AddAsync(newUser, cancellationToken);

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                return (newUser, true);
            }
            catch (DbUpdateException)
            {
                dbContext.Entry(newUser).State = EntityState.Detached;

                var existing = await dbContext.Users
                    .Available()
                    .FirstOrDefaultAsync(
                        u => u.GoogleId == command.GoogleId || u.Email == command.Email,
                        cancellationToken);

                if (existing is null)
                {
                    throw;
                }

                if (existing.GoogleId is null)
                {
                    existing.GoogleId = command.GoogleId;
                    await dbContext.SaveChangesAsync(cancellationToken);
                }

                return (existing, false);
            }
        }
    }

    internal sealed class Validator : ExtendedAbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.GoogleId)
                .NotEmpty();

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();
        }
    }
}