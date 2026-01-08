using Avemepls.Infrastructure.Email;
using Microsoft.Extensions.DependencyInjection;

namespace Avemepls.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IEmailService, ConsoleEmailService>();

        return services;
    }
}