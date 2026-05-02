using Avemepls.Auth.Abstractions.Models;
using Avemepls.Auth.Bearer;
using Avemepls.Auth.Bearer.Abstractions;
using Avemepls.Auth.Domain.Extensions;
using Avemepls.Auth.Domain.Services;
using Avemepls.Core.DataAccess.Extensions;
using Avemepls.Core.Localization;
using Avemepls.Domain.Validators;
using Avemepls.Identity.DataAccess;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Avemepls.Auth.Domain.ViaPassword;

public static class Login
{
    public class Command : IRequest<TokenInformation>
    {
        [DisplayNameLoc("Имя пользователя")]
        public string Username { get; set; }

        [DisplayNameLoc("Пароль")]
        public string Password { get; set; }
    }

    internal sealed class Handler(IdentityDbContext dbContext, ITokenGenerator tokenGenerator, IPublisher publisher, IStringLocalizer<Handler> loc)
        : IRequestHandler<Command, TokenInformation>
    {
        public async Task<TokenInformation> Handle(Command command, CancellationToken cancellationToken)
        {
            var user = await dbContext.Users.Available().FirstOrDefaultAsync(u => u.Username == command.Username, cancellationToken);

            if (user?.PasswordHash is null || !PasswordHasher.VerifyPassword(command.Password, user.PasswordHash))
            {
                throw new ValidationException(loc["Неверные логин или пароль"]);
            }

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
                    Date = DateTimeOffset.Now
                },
                cancellationToken);

            return token;
        }
    }

    public class Validator : ExtendedAbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(u => u.Username).NotEmpty();
            RuleFor(u => u.Password).NotEmpty();
        }
    }
}