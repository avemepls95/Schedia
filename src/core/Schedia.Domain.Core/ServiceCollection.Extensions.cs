using Avemepls.Core.DataAccess.Behaviors;
using Avemepls.Domain.Behaviors;
using Avemepls.Mapster;

using MediatR;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Schedia.Domain.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomainCore(this IServiceCollection services)
    {
        services.AddMapsterAdapter(typeof(ServiceCollectionExtensions).Assembly);
        services.TryAddEnumerable(new ServiceDescriptor(typeof(IPipelineBehavior<,>), typeof(TransactionPipelineBehavior<,>), ServiceLifetime.Scoped));
        services.TryAddEnumerable(new ServiceDescriptor(typeof(IPipelineBehavior<,>), typeof(FluentValidationPipelineBehavior<,>), ServiceLifetime.Scoped));

        return services;
    }
}