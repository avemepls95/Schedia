using Avemepls.Auth.Password.Abstractions;
using Avemepls.Auth.Password.Models;

using MediatR;

namespace Schedia.Auth.Domain.ViaPassword;

public static class RequestPasswordReset
{
    public class Command : RequestPasswordResetRequest, IRequest
    {
    }

    internal sealed class Handler(IAuthService authService) : IRequestHandler<Command>
    {
        public async Task Handle(Command command, CancellationToken cancellationToken)
        {
            await authService.RequestPasswordResetAsync(command, cancellationToken);
        }
    }
}