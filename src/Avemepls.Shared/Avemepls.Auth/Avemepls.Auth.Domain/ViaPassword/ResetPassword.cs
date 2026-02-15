using Avemepls.Auth.Domain.Services;
using Avemepls.Auth.Domain.Validators;
using Avemepls.Core.DataAccess.Behaviors;
using Avemepls.Core.DataAccess.Extensions;
using Avemepls.Core.Extensions;
using Avemepls.Domain.Validators;
using Avemepls.Identity.DataAccess;
using Avemepls.Infrastructure.DateTime;

using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Avemepls.Auth.Domain.ViaPassword;

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
            var resetPasswordRecord = await dbContext.RequestResetPasswordRecords.AsTracking()
                .Include(x => x.User)
                .FirstAsync(u => u.User.Available() && u.Token == command.Token, cancellationToken);

            resetPasswordRecord.User!.PasswordHash = PasswordHasher.HashPassword(command.NewPassword);
            resetPasswordRecord.Token = null;
            resetPasswordRecord.TokenExpiry = null;

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public class Validator<TCommand> : ExtendedAbstractValidator<TCommand>
        where TCommand : Command
    {
        public Validator(
            IDbContextFactory<IdentityDbContext> dbContextFactory,
            IStringLocalizer<Validator<TCommand>> loc,
            IStringLocalizer<PasswordValidator> passwordLoc,
            ICurrentDateTimeProvider currentDateTimeProvider)
        {
            RuleFor(x => x.NewPassword).SetValidator(new PasswordValidator(passwordLoc));

            RuleFor(x => x.Token)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .CustomAsync(async (value, validationContext, cancellationToken) =>
                {
                    await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
                    var user = await dbContext.RequestResetPasswordRecords
                        .Include(x => x.User)
                        .Where(x => x.User.Available() && x.Token == value)
                        .Select(x => new { Id = (int?)x.UserId, x.TokenExpiry })
                        .FirstOrDefaultAsync(cancellationToken);

                    if (user?.Id is null)
                    {
                        validationContext.AddFailure(loc["Пользователь не найден"]);
                        return;
                    }

                    if (user.TokenExpiry < currentDateTimeProvider.UtcNow)
                    {
                        validationContext.AddFailure(loc["Срок ссылки сброса пароля истек. Повторите процедуру заново"]);
                    }
                })
                .When(x => !x.Token.IsNullOrWhiteSpace());
        }
    }
}