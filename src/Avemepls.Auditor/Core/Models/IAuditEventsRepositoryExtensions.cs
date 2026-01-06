namespace Avemepls.Auditor.Core.Models;

public static class AuditEventQueryExtensions
{
    /// <summary>
    /// Prepare query to get entries for entity instance
    /// </summary>
    public static SearchAuditEventsQuery ForEntity<TEntity, TId>(this SearchAuditEventsQuery query, TId id)
    {
        if (query.EntityTypes == null)
            query.EntityTypes = [typeof(TEntity).Name];
        else
            query.EntityTypes = query.EntityTypes.Concat([typeof(TEntity).Name]).ToArray();

        query.EntityId = Convert.ToString(id);

        return query;
    }

    /// <summary>
    /// Prepare query to get entries for inner entity
    /// </summary>
    /// <param name="query">Additional filter parameters (paging, dates, user etc)</param>
    /// <param name="id">Entity id</param>
    public static SearchAuditEventsQuery GetForInnerEntity<TEntity, TId>(this SearchAuditEventsQuery query, TId id)
    {
        query.InnerEntityType = typeof(TEntity).Name;
        query.InnerEntityId = Convert.ToString(id);

        return query;
    }
}