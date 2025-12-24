using Avemepls.Core.Extensions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Avemepls.Core.DataAccess.Extensions;

/// <summary>
/// Extensions for EntityTypeBuilder
/// </summary>
public static class EntityTypeBuilderExtensions
{
    /// <summary>
    /// Makes table and schema to be converted in snake case
    /// </summary>
    /// <param name="builder">Builder instance</param>
    /// <param name="tableName">Name of table in pascal case</param>
    /// <param name="schemaName">Name of schema in pascal case</param>
    /// <typeparam name="TEntity">Type of entity</typeparam>
    public static EntityTypeBuilder<TEntity> ToTableInSnakeCase<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        string tableName,
        string schemaName)
        where TEntity : class
    {
        builder.ToTable(tableName.ToSnakeCase(), schemaName.ToSnakeCase());

        return builder;
    }

    /// <summary>
    /// Makes table and schema to be converted in snake case
    /// </summary>
    /// <param name="builder">Builder instance</param>
    /// <param name="tableName">Name of table in pascal case</param>
    /// <param name="schemaName">Name of schema in pascal case</param>
    public static EntityTypeBuilder ToTableInSnakeCase(
        this EntityTypeBuilder builder,
        string tableName,
        string schemaName)
    {
        builder.ToTable(tableName.ToSnakeCase(), schemaName.ToSnakeCase());

        return builder;
    }
}