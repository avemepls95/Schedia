using Avemepls.Auditor.DataAccess;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Avemepls.Auditor.Domain.Queries;

/// <summary>
/// Собирает значения измерений событий аудита для последующей фильтраци.
/// Выбирает различные значения типов сущностей, систем-источников и типов событий
/// </summary>
public static class GetAuditEventsStats
{
    public class Model
    {
        /// <summary>
        /// Типы событий
        /// </summary>
        public string[] EventTypes { get; set; }

        /// <summary>
        /// Источники событий
        /// </summary>
        public string[] EventSources { get; set; }

        /// <summary>
        /// Типы сущностей
        /// </summary>
        public string[] EntityTypes { get; set; }
    }

    public class Query : IRequest<Model>
    {
        /// <summary>
        /// Type of entity
        /// </summary>
        public string? EntityType { get; set; }

        /// <summary>
        /// ID of entity
        /// </summary>
        public string? EntityId { get; set; }

        /// <summary>
        /// Event sources
        /// </summary>
        public string[]? Sources { get; set; }
    }

    public class Handler(IDbContextFactory<AuditDataContext> contextFactory) : IRequestHandler<Query, Model>
    {
        public async Task<Model> Handle(Query request, CancellationToken cancellationToken)
        {
            await using var ctx = await contextFactory.CreateDbContextAsync(cancellationToken);
            var data = ctx.AuditEvents.AsQueryable();

            if (request.EntityType is not null)
                data = data.Where(e => e.EntityType == request.EntityType);

            if (request.EntityId is not null)
                data = data.Where(e => e.EntityId == request.EntityId);

            if (request.Sources?.Any() is true)
                data = data.Where(e => request.Sources.Contains(e.Source));

            var eventTypes = await data
                .Select(e => e.EventType)
                .Distinct()
                .ToArrayAsync(cancellationToken);

            var sources = await data
                .Where(e => e.Source != null)
                .Select(e => e.Source!)
                .Distinct()
                .ToArrayAsync(cancellationToken);

            var entityTypes = await data
                .Where(e => e.EntityType != null)
                .Select(e => e.EntityType!)
                .Distinct()
                .ToArrayAsync(cancellationToken);

            return new Model
            {
                EventTypes = eventTypes,
                EventSources = sources,
                EntityTypes = entityTypes
            };
        }
    }
}