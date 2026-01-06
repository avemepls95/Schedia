using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Avemepls.Auditor.DataAccess.DbContextAuditor;

public class AuditableEntry(EntityState state, EntityEntry entityEntry)
{
    /// <summary>
    /// Entity state
    /// </summary>
    public EntityState State { get; } = state;

    /// <summary>
    /// Entry from context
    /// </summary>
    public EntityEntry EntityEntry { get; } = entityEntry;

    /// <summary>
    /// Entity instance
    /// </summary>
    public object Entity => EntityEntry.Entity;

    public override string ToString() => $"{State}({Entity.GetType().Name})";
}