using Avemepls.Auditor.DataAccess.Models;

using Microsoft.EntityFrameworkCore;

namespace Avemepls.Auditor.DataAccess;

public class AuditDataContext : DbContext
{
    public DbSet<AuditEvent> AuditEvents { get; set; }

    protected AuditDataContext()
    {
    }

    public AuditDataContext(DbContextOptions<AuditDataContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuditDataContext).Assembly);
    }
}