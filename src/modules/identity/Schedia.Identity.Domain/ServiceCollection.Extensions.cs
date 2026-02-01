using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

using ServiceScan.SourceGenerator;

namespace Schedia.Identity.Domain;

public static partial class ServiceCollectionExtensions
{
    [GenerateServiceRegistrations(AssignableTo = typeof(IValidator<>), Lifetime = ServiceLifetime.Transient)]
    public static partial IServiceCollection AddFluentValidators(this IServiceCollection services);

    public static IServiceCollection AddSchediaIdentityDomain(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));
        serviceCollection.AddFluentValidators();

        return serviceCollection;
    }
}