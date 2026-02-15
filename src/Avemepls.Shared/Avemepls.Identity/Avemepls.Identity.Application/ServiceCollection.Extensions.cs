using Avemepls.Identity.Domain;
using Avemepls.ServiceBus;

using MediatR;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using ServiceScan.SourceGenerator;

namespace Avemepls.Identity.Application;

public static partial class ServiceCollectionExtensions
{
    [GenerateServiceRegistrations(AssignableTo = typeof(INotificationHandler<>), AsImplementedInterfaces = true, Lifetime = ServiceLifetime.Transient)]
    public static partial IServiceCollection AddMediatRHandlers(this IServiceCollection services);

    public static IServiceCollection AddIdentityApplication(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddIdentityDomain(configuration);
        serviceCollection.AddConsumers(typeof(ServiceCollectionExtensions).Assembly);
        serviceCollection.AddMediatRHandlers();

        return serviceCollection;
    }
}