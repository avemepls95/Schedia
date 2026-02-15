using Avemepls.Core.Models;

namespace Avemepls.Auditor.DataAccess.DbContextAuditor;

/// <summary>
/// Настройки для <see cref="DbContextAuditorInterceptor"/>
/// </summary>
public class AuditorConfig
{
    private readonly HashSet<Type> _auditableTypes = [];

    public IEnumerable<Type> AuditableTypes => _auditableTypes;

    public AuditorConfig AddAuditableType<T>()
    {
        _auditableTypes.Add(typeof(T));

        return this;
    }

    public AuditorConfig AddAuditableTypesFromAssembly<T>()
    {
        var aEntityTypes = typeof(T)
            .Assembly.GetTypes()
            .Where(x =>
                typeof(IHasDateDeleted).IsAssignableFrom(x)
                || typeof(IHasDateCreated).IsAssignableFrom(x)
                || typeof(IHasDateUpdated).IsAssignableFrom(x)
                || typeof(IHasUserCreated).IsAssignableFrom(x)
                || typeof(IHasUserDeleted).IsAssignableFrom(x)
            )
            .Where(x => x.IsClass && !x.IsAbstract)
            .ToArray();

        foreach (var type in aEntityTypes)
        {
            _auditableTypes.Add(type);
        }

        return this;
    }
}