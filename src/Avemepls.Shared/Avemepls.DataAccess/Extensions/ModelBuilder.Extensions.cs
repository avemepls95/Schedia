using System.Reflection;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Avemepls.Core.DataAccess.Extensions;

public static class ModelBuilderExtensions
{
    /// <summary>
    /// Extension for pass DatabaseFacade in EntityConfigurationType. Need more cases for entity configuration.
    /// For example: configure array field for memory db context in <see cref="IEntityTypeConfiguration{TEntity}"/>
    /// </summary>
    /// <param name="modelBuilder"><see cref="ModelBuilder"/></param>
    /// <param name="databaseFacade">Pass from DbContext.Database</param>
    /// <param name="assemblies">Array of assemblies with configurations</param>
    /// <exception cref="ArgumentNullException">When args is null</exception>
    public static void ApplyConfigurationsFromAssemblies(
        this ModelBuilder modelBuilder,
        DatabaseFacade databaseFacade,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        foreach (var assembly in assemblies)
        {
            var configurations = assembly
                .GetTypes()
                .Where(t => t is { IsAbstract: false, IsGenericTypeDefinition: false })
                .Where(t => Array.Exists(t.GetInterfaces(),
                                         i => i.IsGenericType &&
                                              i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)))
                .ToList();

            foreach (var configuration in configurations)
            {
                var listArgs = new List<object>();

                if (configuration.HasConstructorArguments<DatabaseFacade>())
                {
                    listArgs.Add(databaseFacade);
                }

                if (configuration.HasConstructorArguments<ModelBuilder>())
                {
                    listArgs.Add(modelBuilder);
                }

                var entityConfigurationInstance = listArgs.Any()
                    ? Activator.CreateInstance(configuration, listArgs.ToArray())
                    : Activator.CreateInstance(configuration);

                var entityType = configuration.GetInterfaces()
                    .Single(i => i.IsGenericType &&
                                 i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
                    .GetGenericArguments()[0];

                var methodInfo = typeof(ModelBuilder)
                    .GetMethods()
                    .Single(e => e is { Name: nameof(ModelBuilder.ApplyConfiguration), ContainsGenericParameters: true }
                                 && e.GetParameters()
                                     .SingleOrDefault()
                                     ?.ParameterType
                                     .GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>));

                methodInfo.MakeGenericMethod(entityType)
                    .Invoke(modelBuilder, [entityConfigurationInstance]);
            }
        }
    }

    private static bool HasConstructorArguments<TArg>(this Type type)
    {
        return Array.Exists(type.GetConstructors(), x => Array.Exists(x.GetParameters(), w => w.ParameterType == typeof(TArg)));
    }
}