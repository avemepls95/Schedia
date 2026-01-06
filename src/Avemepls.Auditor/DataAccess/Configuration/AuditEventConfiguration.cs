using Avemepls.Auditor.DataAccess.Models;
using Avemepls.Core.DataAccess.Extensions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Avemepls.Auditor.DataAccess.Configuration;

public class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        builder.ToTableInSnakeCase(nameof(AuditEvent), "audit");
        builder.Property(a => a.Source).HasMaxLength(64);
        builder.Property(a => a.EventType).HasMaxLength(64);
        builder.Property(a => a.EntityType).HasMaxLength(64);
        builder.Property(a => a.EntityId).HasMaxLength(36);
        builder.Property(a => a.InnerEntityType).HasMaxLength(64);
        builder.Property(a => a.InnerEntityId).HasMaxLength(36);

        builder.HasIndex(a => a.Source);
        builder.HasIndex(a => a.DateTime);
        builder.HasIndex(a => a.UserId);

        builder.HasIndex(a => new
        {
            a.EntityType, a.EntityId
        });

        builder.HasIndex(a => new
        {
            a.InnerEntityType,
            a.InnerEntityId
        });
    }
}