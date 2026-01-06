using Avemepls.Auditor.Interfaces;
using Avemepls.Security.Principal;

using Microsoft.AspNetCore.Http;
using Microsoft.VisualBasic;

using Constants = Avemepls.Security.Principal.Constants;

namespace Avemepls.Auditor;

public class AuditScopeFactory(
    IServiceProvider serviceProvider,
    IHttpContextAccessor? contextAccessor,
    IPrincipalAccessor? principalAccessor)
    : IAuditScopeFactory
{
    public async Task<AuditScope> Create(Action<AuditScopeOptions>? optionsBuilder, CancellationToken cancellationToken)
    {
        var options = await BuildOptions();

        optionsBuilder?.Invoke(options);

        var scope = new AuditScope(options, serviceProvider);

        return scope;
    }

    private async Task<AuditScopeOptions> BuildOptions()
    {
        var options = new AuditScopeOptions();

        var httpContext = contextAccessor?.HttpContext;

        if (httpContext is not null)
        {
            options.WithIpAddress(httpContext.Connection.RemoteIpAddress);
        }

        if (options.CorrelationId is null)
        {
            if (httpContext?.Request.Headers.TryGetValue("x-correlation-id", out var correlationId) == true)
                options.WithCorrelationId(correlationId.ToString());
            else
                options.WithCorrelationId(Guid.NewGuid().ToString());
        }

        if (principalAccessor is not null)
        {
            var principal = await principalAccessor.GetPrincipal();

            if (options.UserId is null && principal.TryGetClaimValue<string>(Constants.ClaimTypes.UserId, out var userId))
            {
                options.WithUserId(userId);
            }

            if (options.UserName is null && principal.TryGetClaimValue<string>(Constants.ClaimTypes.Login, out var userName))
            {
                options.WithUserName(userName);
            }
        }

        return options;
    }
}