using System.Reflection.Metadata;

using Avemepls.Core.DataAccess.DataContext;
using Avemepls.Core.DataAccess.Extensions;

using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Schedia.DataAccess;

/// <summary>
/// Основной контекст баз данных с базовыми таблицами
/// </summary>
public partial class SchediaDbContext(DbContextOptions<SchediaDbContext> options) : DbContext(options)
{

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssemblies(
            Database
        );

        builder.DisableCascadeDeleteGlobally();

        builder.EnumsAsString();
        builder.HasPostgresExtension("citext");
        builder.HasPostgresExtension("uuid-ossp");
        builder.HasPostgresExtension("pg_trgm");
        builder.AddInboxStateEntity(cfg => cfg.ToTableInSnakeCase(nameof(InboxState), "service_bus"));
        builder.AddOutboxMessageEntity(cfg => cfg.ToTableInSnakeCase(nameof(OutboxMessage), "service_bus"));
        builder.AddOutboxStateEntity(cfg => cfg.ToTableInSnakeCase(nameof(OutboxState), "service_bus"));

        builder.Ignore<CustomAttributeValue<int>>();
    }

    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseProjectables();
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.UseSnakeCaseNamingConvention();

        optionsBuilder.ConfigureWarnings(w =>
        {
            w.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning);
            w.Throw(RelationalEventId.MultipleCollectionIncludeWarning);
        });
    }
}