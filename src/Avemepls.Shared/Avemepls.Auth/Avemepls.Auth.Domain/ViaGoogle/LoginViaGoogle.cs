using Avemepls.Auth.Abstractions.Models;
using Avemepls.Auth.Bearer;
using Avemepls.Auth.Bearer.Abstractions;
using Avemepls.Core.DataAccess.Behaviors;
using Avemepls.Core.DataAccess.Extensions;
using Avemepls.Identity.DataAccess;
using Avemepls.Identity.DataAccess.Models;
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
        public string? DisplayName { get; set; }
    }

    internal sealed class Handler(
        IdentityDbContext dbContext,
        ITokenGenerator tokenGenerator,
        IPublisher publisher) : IRequestHandler<Command, TokenInformation>
    {
        public async Task<TokenInformation> Handle(Command command, CancellationToken cancellationToken)
        {
            var user = await dbContext.Users
                .Available()
                .FirstOrDefaultAsync(u => u.GoogleId == command.GoogleId, cancellationToken);

            if (user is null)
            {
                user = await dbContext.Users
                    .Available()
                    .FirstOrDefaultAsync(u => u.Email == command.Email, cancellationToken);

                if (user is not null)
                {
                    user.GoogleId = command.GoogleId;
                }
                else
                {
                    user = new User
                    {
                        Username = command.DisplayName ?? command.Email,
                        Email = command.Email,
                        GoogleId = command.GoogleId,
                        IsActive = true,
                        EmailConfirmed = true
                    };

                    await dbContext.Users.AddAsync(user, cancellationToken);
                }

                await dbContext.SaveChangesAsync(cancellationToken);
            }

            var token = tokenGenerator.Create(user.Id);

            await publisher.Publish(
                new UserLoginNotification
                {
                    Id = user.Id,
                    Date = DateTimeOffset.Now
                },
                cancellationToken);

            return token;
        }
    }
}