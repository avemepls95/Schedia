using Avemepls.Auth.Bearer;
using Avemepls.Auth.Bearer.Abstractions;
using Avemepls.Core.DataAccess.Extensions;
using Avemepls.Domain.Validators;
using Avemepls.Identity.DataAccess;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Schedia.Auth.Domain.Services;

namespace Schedia.Auth.Domain.ViaPassword;

public static class Auth
{
    public class Command : IRequest<TokenInformation>
    {
        public string Username { get; set; }

        public string Password { get; set; }
    }

    internal sealed class Handler(IdentityDbContext dbContext, ITokenGenerator tokenGenerator, IStringLocalizer<Handler> loc)
        : IRequestHandler<Command, TokenInformation>
    {
        public async Task<TokenInformation> Handle(Command command, CancellationToken cancellationToken)
        {
            var user = await dbContext.Users.Available().FirstOrDefaultAsync(u => u.Username == command.Username, cancellationToken);

            if (user == null || !PasswordHasher.VerifyPassword(command.Password, user.PasswordHash))
            {
                throw new ValidationException(loc["Неверные логин или пароль"]);
            }

            var token = tokenGenerator.Create(user.Id);

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