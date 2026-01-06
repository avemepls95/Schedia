using System.Reflection;

using Avemepls.Core.DataAccess.Extensions;
using Avemepls.Core.Extensions;
using Avemepls.Core.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Avemepls.Core.DataAccess.Configuration;

public abstract class BaseConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : class, IHasId<TEntity>
{
    private static readonly string DefaultTableName = typeof(TEntity).Name;
    private static readonly string DefaultSchemaName = typeof(TEntity).Namespace!.Split('.')[1];

    private static readonly string[] FieldsAtFirst =
    [
        nameof(IHasId<>.Id),
        nameof(IHasName.Name),
        nameof(IHasCode.Code),
        "Description",
        nameof(IHasIsActive.IsActive),
        nameof(IHasSortOrder.SortOrder)
    ];

    private static readonly string[] FieldsToEnd =
    [
        nameof(IHasDateCreated.DateCreated),
        nameof(IHasUserCreated.UserCreatedId),
        nameof(IHasDateModified.DateModified),
        nameof(IHasDateDeleted.DateDeleted),
        nameof(IHasUserDeleted.UserDeletedId)
    ];

    private static readonly PropertyInfo[] PropertyInfos =
    [
        .. typeof(TEntity)
            .GetProperties()
            .Where(x => x is { CanRead: true, CanWrite: true } && (x.DeclaringType is null || !x.DeclaringType.IsAbstract))
            .OrderBy(GetPriority)
    ];

    protected virtual void ConfigureCore(EntityTypeBuilder<TEntity> builder)
    {
    }

    protected virtual string GetSchemaName() => DefaultSchemaName;

    public void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.ToTableInSnakeCase(DefaultTableName, GetSchemaName());

        builder.HasKey(x => x.Id);

        ConfigureCore(builder);

        SortColumns(builder);
    }

    private static void SortColumns(EntityTypeBuilder<TEntity> builder)
    {
        var sortOrder = 1;

        builder.Property(t => t.Id).HasColumnOrder(sortOrder);

        var ownedProperties = builder.Metadata.GetNavigations()
            .Where(x => x is { IsEagerLoaded: true, ForeignKey.IsOwnership: true }).Select(x => x)
            .ToArray();

        foreach (var property in PropertyInfos)
        {
            var order = ++sortOrder;

            var getMethod = property.GetGetMethod();

            if (getMethod is not null && !getMethod.IsVirtual)
            {
                var ownedProperty = ownedProperties.Find(x => x.TargetEntityType.ClrType == property.PropertyType);

                if (ownedProperty is not null)
                {
                    builder.OwnsOne(
                        property.PropertyType,
                        ownedProperty.Name,
                        navigationBuilder =>
                        {
                            foreach (var propertyInOwned in property.PropertyType.GetProperties().Where(x => x is { CanRead: true, CanWrite: true }))
                            {
                                navigationBuilder.Property(propertyInOwned.Name).HasColumnOrder(order++);
                            }
                        }
                    );

                    sortOrder = order;
                }
                else
                {
                    builder.Property(property.Name).HasColumnOrder(order);
                }
            }
        }
    }

    private static (int Group, int Order) GetPriority(PropertyInfo prop)
    {
        var indexAtFirst = Array.IndexOf(FieldsAtFirst, prop.Name);

        if (indexAtFirst != -1)
        {
            return (-1, indexAtFirst);
        }

        var indexToEnd = Array.IndexOf(FieldsToEnd, prop.Name);

        return indexToEnd != -1 ? (1, indexToEnd) : (0, 0);
    }
}