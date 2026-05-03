using Avemepls.Infrastructure.Email;
using Avemepls.Infrastructure.RateLimit;
using Microsoft.Extensions.DependencyInjection;

namespace Avemepls.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IEmailService, FakeEmailService>();
        services.AddSingleton<IRateLimiter, DistributedCacheRateLimiter>();

        return services;
    }
}