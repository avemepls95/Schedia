using Avemepls.Core.DataAccess.Behaviors;
using Avemepls.Core.DataAccess.Extensions;
using Avemepls.Core.Models;
using Avemepls.Identity.DataAccess;
using Avemepls.Identity.DataAccess.Models;
using Avemepls.Security.Principal;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Schedia.Auth.Domain.Services;
using Schedia.Auth.Domain.Validators;

namespace Schedia.Auth.Domain.ViaPassword;

[Transaction]
public static class ChangePassword
{
    public class Command : IRequest
    {
        public string OldPassword { get; set; }

        public string NewPassword { get; set; }
    }

    internal sealed class Handler(IdentityDbContext dbContext, IPrincipalAccessor principalAccessor)
        : IRequestHandler<Command>
    {
        public async Task Handle(Command command, CancellationToken cancellationToken)
        {
            var userId = (await principalAccessor.GetPrincipal()).GetId()!.Value;
            var user = await dbContext.Users.AsTracking().Available().FirstOrDefaultAsync(u => u.Id == new Id<User>(userId), cancellationToken);
            if (user == null || !PasswordHasher.VerifyPassword(command.OldPassword, user.PasswordHash))
            {
                throw new ValidationException("User with specified old password not found");
            }

            user.PasswordHash = PasswordHasher.HashPassword(command.NewPassword);

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    internal sealed class RegisterRequestValidator : AbstractValidator<Command>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.OldPassword).NotEmpty();
            RuleFor(x => x.NewPassword).SetValidator(new PasswordValidator());
        }
    }
}