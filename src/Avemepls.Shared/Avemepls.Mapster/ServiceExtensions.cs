using System.Linq.Expressions;
using System.Reflection;

using Avemepls.Core.Mapping;

using Mapster;
using Mapster.Utils;

using MapsterMapper;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using IMapper = Avemepls.Core.Mapping.IMapper;
using IMapsterMapper = MapsterMapper.IMapper;

namespace Avemepls.Mapster;

public static class ServiceExtensions
{
    /// <summary>
    /// Adds automapper and initializes mapping adapter
    /// </summary>
    public static IServiceCollection AddMapsterAdapter(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        var registerTypes = assemblies.SelectMany(assembly => assembly.GetLoadableTypes()
                                                      .Where(x => typeof(IRegister).GetTypeInfo()
                                                                      .IsAssignableFrom(x.GetTypeInfo()) &&
                                                                  x.GetTypeInfo().IsClass &&
                                                                  !x.GetTypeInfo().IsAbstract))
            .ToList();

        foreach (var registerType in registerTypes)
        {
            services.TryAddEnumerable(
                new ServiceDescriptor(typeof(IRegister), registerType, ServiceLifetime.Transient));
        }

        services.TryAddSingleton(typeof(TypeAdapterConfig),
                                 serviceProvider =>
                                 {
                                     var options = serviceProvider.GetRequiredService<IOptions<MapperConfig>>().Value ??
                                                   new MapperConfig();

                                     var registers = serviceProvider.GetServices<IRegister>();
                                     var config = new TypeAdapterConfig();
                                     config.Default.ShallowCopyForSameType(true);
                                     config.Default.Settings.EnableNonPublicMembers = true;
                                     config.AllowImplicitSourceInheritance = true;
                                     config.AllowImplicitDestinationInheritance = true;
                                     config.RequireDestinationMemberSource = options.ValidationEnabled;

                                     if (options.EmptyStringToNull)
                                     {
                                         config.Default.AddDestinationTransform(NullWhenEmpty);
                                     }

                                     config.Apply(registers);

                                     return config;
                                 });

        services.TryAddScoped<IMapsterMapper, ServiceMapper>();
        services.TryAddScoped<IMapper, MapsterAdapter>();
        services.AddOptions();

        return services;
    }

    private static Expression<Func<string?, string?>> NullWhenEmpty => str => string.IsNullOrWhiteSpace(str)
        ? null
        : str;

    public static IServiceCollection ConfigureMapsterAdapter(
        this IServiceCollection services,
        Action<MapperConfig> configure)
    {
        services.AddOptions();
        services.Configure(configure);

        return services;
    }
}