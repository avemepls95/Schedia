using Avemepls.Auditor.Commands;
using Avemepls.Auditor.DataAccess;
using Avemepls.Auditor.Domain.Queries;
using Avemepls.Auditor.Interfaces;
using Avemepls.Security;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Avemepls.Auditor;

public static class Extensions
{
    /// <summary>
    /// Registers auditing pipelines and audit database context
    /// </summary>
    public static IServiceCollection AddAuditor(
        this IServiceCollection serviceCollection,
        Action<DbContextOptionsBuilder> contextOptionsBuilder)
    {
        serviceCollection.AddDbContextFactory<AuditDataContext>(contextOptionsBuilder);
        serviceCollection.AddDbContext<AuditDataContext>(contextOptionsBuilder);
        serviceCollection.AddScoped<IAuditScopeFactory, AuditScopeFactory>();
        serviceCollection.AddScoped<IAuditEventsRepository, AuditEventsRepository>();
        serviceCollection.AddMediatR(c => c.RegisterServicesFromAssemblyContaining<SearchAuditEventsQueryHandler>());
        serviceCollection.AddScoped(typeof(IPipelineBehavior<,>), typeof(CommandProcessingAuditorBehavior<,>));
        serviceCollection.AddPermissions(x => x.AddAssemblies(typeof(SearchAuditEventsQueryHandler).Assembly));

        return serviceCollection;
    }
}