using System.Reflection;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Avemepls.Core.DataAccess.DataContext;

/// <summary>
/// Extensions for the <see cref="ModelBuilder"/>.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Disable cascade delete on all relations
    /// </summary>
    /// <param name="modelBuilder">Database model builder</param>
    /// <param name="exceptions">Exception types for cascade delete disabling</param>
    public static ModelBuilder DisableCascadeDeleteGlobally(this ModelBuilder modelBuilder, params Type[] exceptions)
    {
        var cascadeForeignKeys = modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetForeignKeys())
            .Where(fk => !fk.IsOwnership
                         && fk.DeleteBehavior == DeleteBehavior.Cascade
                         && !exceptions.Contains(fk.PrincipalEntityType.ClrType));

        foreach (var fk in cascadeForeignKeys)
        {
            fk.DeleteBehavior = DeleteBehavior.Restrict;
        }

        return modelBuilder;
    }

    /// <summary>
    /// Converts all enums to string fields. Enums with Flags attributes are not supported and will not be affected.
    /// </summary>
    /// <param name="modelBuilder">Database model builder</param>
    public static ModelBuilder EnumsAsString(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                var isEnum = property.ClrType.IsEnum || Nullable.GetUnderlyingType(property.ClrType)?.IsEnum is true;

                if (isEnum)
                {
                    var enumType = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
                    var attribute = enumType.GetCustomAttribute<FlagsAttribute>();

                    if (attribute == null)
                    {
                        var type = typeof(EnumToStringConverter<>).MakeGenericType(enumType);
                        var converter = Activator.CreateInstance(type) as ValueConverter;
                        property.SetValueConverter(converter);
                    }
                }
            }
        }

        return modelBuilder;
    }

    // По какой-то причине условие IsAbstract в демо-проекте приводит к ошибке. В случае, если его убрать
    private static Type? GetFirstTypeInHierarchyImplementing<T>(this Type type)
    {
        if (typeof(T).IsAssignableFrom(type) && (type.BaseType == null || type.BaseType.IsAbstract || !typeof(T).IsAssignableFrom(type.BaseType)))
            return type;

        return type.BaseType?.GetFirstTypeInHierarchyImplementing<T>();
    }
}