using Avemepls.Auth.Bearer;
using Avemepls.Mapster;

using FluentValidation;

using MediatR;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using ServiceScan.SourceGenerator;

namespace Schedia.Auth.Domain;

public static partial class ServiceCollectionExtensions
{
    [GenerateServiceRegistrations(AssignableTo = typeof(IValidator<>), Lifetime = ServiceLifetime.Transient)]
    public static partial IServiceCollection AddFluentValidators(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(IRequestHandler<>), Lifetime = ServiceLifetime.Transient)]
    [GenerateServiceRegistrations(AssignableTo = typeof(IRequestHandler<,>), Lifetime = ServiceLifetime.Transient)]
    public static partial IServiceCollection AddMediatRHandlers(this IServiceCollection services);

    public static IServiceCollection AddSchediaAuthDomain(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var oAuthOptions = configuration.GetSection("IdentityOptions").Get<OAuthOptions>()!;

        serviceCollection
            .AddJwtBearerTokenAuth(oAuthOptions);

        serviceCollection.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));
        serviceCollection.AddFluentValidators();
        serviceCollection.AddMapsterAdapter(typeof(ServiceCollectionExtensions).Assembly);

        return serviceCollection;
    }
}