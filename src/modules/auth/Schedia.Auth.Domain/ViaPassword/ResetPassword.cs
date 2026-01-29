using Avemepls.Core.DataAccess.Behaviors;
using Avemepls.Core.DataAccess.Extensions;
using Avemepls.Core.Extensions;
using Avemepls.Domain.Validators;
using Avemepls.Identity.DataAccess;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Schedia.Auth.Domain.Services;
using Schedia.Auth.Domain.Validators;

namespace Schedia.Auth.Domain.ViaPassword;

[Transaction]
public static class ResetPassword
{
    public class Command : IRequest
    {
        public string Token { get; set; }

        public string NewPassword { get; set; }
    }

    internal sealed class Handler(IdentityDbContext dbContext)
        : IRequestHandler<Command>
    {
        public async Task Handle(Command command, CancellationToken cancellationToken)
        {
            var user = await dbContext.Users.AsTracking().Available()
                .FirstAsync(u => u.PasswordResetToken == command.Token, cancellationToken);

            user.PasswordHash = PasswordHasher.HashPassword(command.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;

            await dbContext.SaveChangesAsync(cancellationToken);
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
            RuleFor(x => x.NewPassword).SetValidator(new PasswordValidator(passwordLoc));

            RuleFor(x => x.Token)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .CustomAsync(async (value, validationContext, cancellationToken) =>
                {
                    await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
                    var user = await dbContext.Users
                        .Available()
                        .Where(x => x.PasswordResetToken == value)
                        .Select(x => new { Id = (int?)x.Id, x.PasswordResetTokenExpiry })
                        .FirstOrDefaultAsync(cancellationToken);

                    if (user?.Id is null)
                    {
                        validationContext.AddFailure(loc["Пользователь не найден"]);
                        return;
                    }

                    if (user.PasswordResetTokenExpiry < DateTimeOffset.UtcNow)
                    {
                        validationContext.AddFailure(loc["Срок ссылки сброса пароля истек. Повторите процедуру заново"]);
                    }
                })
                .When(x => !x.Token.IsNullOrWhiteSpace());
        }
    }
}