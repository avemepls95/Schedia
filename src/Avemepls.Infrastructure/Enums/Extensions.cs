using Microsoft.Extensions.DependencyInjection;

namespace Avemepls.Infrastructure.Enums;

public static class Extensions
{
    public static IServiceCollection AddEnumsService(
        this IServiceCollection services,
        Action<EnumsServiceOptions> setupAction)
    {
        services.Configure(setupAction);

        return services.AddSingleton<EnumsService>();
    }
}