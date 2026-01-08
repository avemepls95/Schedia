using Avemepls.Auth.Password.Abstractions;
using Avemepls.Auth.Password.Models;
using Avemepls.Core.Mapping;

using MediatR;

namespace Schedia.Auth.Domain.ViaPassword;

public static class Register
{
    public class Command : RegisterRequest, IRequest
    {
    }

    internal sealed class Handler(IAuthService authService) : IRequestHandler<Command>
    {
        public async Task Handle(Command command, CancellationToken cancellationToken)
        {
            await authService.Register(command, cancellationToken);
        }
    }
}