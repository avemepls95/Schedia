using Avemepls.Auth.Password.Abstractions;
using Avemepls.Auth.Password.Models;
using Avemepls.Core.Mapping;

using FluentValidation;

using MediatR;

namespace Schedia.Auth.Domain.ViaPassword;

public static class CheckUserViaPassword
{
    public class Query : AuthenticateRequest, IRequest<bool>
    {
    }

    internal sealed class Handler(IAuthService authService, IMapper mapper) : IRequestHandler<Query, bool>
    {
        public async Task<bool> Handle(Query query, CancellationToken cancellationToken)
        {
            return await authService.Authenticate(query, cancellationToken) != null;
        }
    }

    internal sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Username).NotEmpty();
            RuleFor(x => x.Password).NotEmpty();
        }
    }
}