using Avemepls.Core.Models;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Schedia.Core;

public static class Extensions
{
    public static IServiceCollection AddSchediaCore(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var identityOptions = configuration.GetSection("AppOptions").Get<AppOptions>()!;
        serviceCollection.AddSingleton(identityOptions);

        return serviceCollection;
    }
}