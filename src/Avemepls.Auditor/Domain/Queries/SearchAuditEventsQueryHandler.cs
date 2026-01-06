using Avemepls.Auditor.Core.Models;
using Avemepls.Auditor.DataAccess;
using Avemepls.Auditor.DataAccess.Models;
using Avemepls.Core.Mapping;
using Avemepls.Domain.Filters;
using Avemepls.Domain.Queries;

using Microsoft.EntityFrameworkCore;

namespace Avemepls.Auditor.Domain.Queries;

/// <summary>
/// Handler to search audit events
/// </summary>
public class SearchAuditEventsQueryHandler(
    IMapper mapper,
    IDbContextFactory<AuditDataContext> context,
    IEnumerable<IQueryableModifier<AuditEvent>>? modifiers)
    :
        ListQueryHandler<AuditEventModel, AuditEvent, AuditDataContext, SearchAuditEventsQuery>(mapper, context, modifiers)
{
    protected override async Task<IQueryable<AuditEvent>> BuildQuery(
        AuditDataContext context,
        SearchAuditEventsQuery request,
        CancellationToken cancellationToken)
    {
        var query = await base.BuildQuery(context, request, cancellationToken);

        if (request.Sources?.Any() is true)
            query = query.Where(e => request.Sources.Contains(e.Source));

        if (request.EventTypes?.Any() is true)
            query = query.Where(q => request.EventTypes.Contains(q.EventType));

        if (request.Severities?.Any() is true)
            query = query.Where(q => request.Severities.Contains(q.Severity));

        if (request.EntityTypes?.Any() is true)
            query = query.Where(q => request.EntityTypes.Contains(q.EntityType));

        if (request.EntityId is not null)
            query = query.Where(q => q.EntityId == request.EntityId);

        if (request.InnerEntityType is not null)
            query = query.Where(q => q.InnerEntityType == request.InnerEntityType);

        if (request.InnerEntityId is not null)
            query = query.Where(q => q.InnerEntityId == request.InnerEntityId);

        if (request.UserId is not null)
            query = query.Where(q => q.UserId == request.UserId);

        if (request.DateFrom is not null)
            query = query.Where(q => q.DateTime >= request.DateFrom);

        if (request.DateTo is not null)
            query = query.Where(q => q.DateTime < request.DateTo);

        if (!string.IsNullOrWhiteSpace(request.Query))
            query = query.Where(q => q.Message!.ToUpper().Contains(request.Query.ToUpper()));

        return query.OrderByDescending(q => q.DateTime);
    }
}