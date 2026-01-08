using Avemepls.Auth.Bearer;
using Avemepls.Auth.Password;
using Avemepls.Mapster;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Schedia.Auth.Domain;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSchediaAuth(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var oAuthOptions = configuration.GetSection("IdentityOptions").Get<OAuthOptions>()!;

        serviceCollection
            .AddJwtBearerTokenAuth(oAuthOptions)
            .AddPasswordAuth();

        serviceCollection.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));
        serviceCollection.AddMapsterAdapter(typeof(ServiceCollectionExtensions).Assembly);

        return serviceCollection;
    }
}