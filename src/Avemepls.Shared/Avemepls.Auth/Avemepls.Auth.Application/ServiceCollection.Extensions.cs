using Avemepls.Auth.Domain;
using Avemepls.ServiceBus;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceScan.SourceGenerator;

namespace Avemepls.Auth.Application;

public static partial class ServiceCollectionExtensions
{
    [GenerateServiceRegistrations(AssignableTo = typeof(INotificationHandler<>), AsImplementedInterfaces = true, Lifetime = ServiceLifetime.Transient)]
    public static partial IServiceCollection AddMediatRHandlers(this IServiceCollection services);

    public static IServiceCollection AddAuthApplication(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddAuthDomain(configuration);
        serviceCollection.AddConsumers(typeof(ServiceCollectionExtensions).Assembly);
        serviceCollection.AddMediatRHandlers();

        return serviceCollection;
    }
}