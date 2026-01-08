namespace Schedia.Domain.Core;

// #PoIgnore#
#pragma warning disable S2094
public static class ServiceCollectionExtensions
#pragma warning restore S2094
{
    // public static IServiceCollection AddDomainCore(this IServiceCollection services)
    // {
    //     services.AddMapsterAdapter(typeof(BaseRules).Assembly);
    //     services.TryAddEnumerable(new ServiceDescriptor(typeof(IPipelineBehavior<,>), typeof(TransactionPipelineBehavior<,>), ServiceLifetime.Scoped));
    //     services.TryAddEnumerable(new ServiceDescriptor(typeof(IPipelineBehavior<,>), typeof(FluentValidationPipelineBehavior<,>), ServiceLifetime.Scoped));
    //
    //     services.TryAddScoped<UserConfirmationContext>();
    //
    //     return services;
    // }
}