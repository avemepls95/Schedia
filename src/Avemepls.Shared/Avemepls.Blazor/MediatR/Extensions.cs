using Microsoft.Extensions.DependencyInjection;

namespace Avemepls.Blazor.MediatR;

public static class Extensions
{
    /// <summary>
    /// Register ScopedMediator which creating scope on every request
    /// </summary>
    public static IServiceCollection AddScopedMediator(this IServiceCollection services)
    {
        services.AddSingleton<IScopedMediator, ScopedMediator>();

        return services;
    }
}