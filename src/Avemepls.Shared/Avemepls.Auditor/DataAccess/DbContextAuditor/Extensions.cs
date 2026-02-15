using Microsoft.Extensions.DependencyInjection;

namespace Avemepls.Auditor.DataAccess.DbContextAuditor;

public static class Extensions
{
    public static IServiceCollection ConfigureAuditor(this IServiceCollection services, Action<AuditorConfig> configure)
    {
        services.Configure(configure);

        return services;
    }
}