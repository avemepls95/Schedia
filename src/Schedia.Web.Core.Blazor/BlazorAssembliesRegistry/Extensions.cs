using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

namespace Schedia.Web.Core.Blazor.BlazorAssembliesRegistry;

public static class ServiceCollectionScanExtensions
{
    public static IServiceCollection AddBlazorPages(this IServiceCollection services, params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            BlazorAssembliesRegistry.AssembliesList.Add(assembly);
        }

        return services;
    }
}