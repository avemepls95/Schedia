using MediatR;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Schedia.Application.Core;
using Schedia.Auth.Domain;

using ServiceScan.SourceGenerator;

namespace Schedia.Auth.Application;

public static partial class ServiceCollectionExtensions
{
    [GenerateServiceRegistrations(AssignableTo = typeof(INotificationHandler<>), AsImplementedInterfaces = true, Lifetime = ServiceLifetime.Transient)]
    public static partial IServiceCollection AddMediatRHandlers(this IServiceCollection services);

    public static IServiceCollection AddSchediaAuthApplication(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddSchediaAuthDomain(configuration);
        serviceCollection.AddConsumers(typeof(ServiceCollectionExtensions).Assembly);
        serviceCollection.AddMediatRHandlers();

        return serviceCollection;
    }
}