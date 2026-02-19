# Schedia — Project Architecture

## Overview
Schedia is a cross-platform application with web (Blazor Server) and mobile (MAUI) clients. Both share a common UI layer — `Schedia.Web.Base` (Razor Class Library).

Runnable projects:
- `src/Schedia.Web.Blazor` — web server (ASP.NET Core)
- `src/Schedia.Web.MAUI` — mobile app (iOS + Android)

## Project Structure

### Two project families
- **Avemepls.\*** — reusable framework (platform layer), contains no Schedia business logic
- **Schedia.\*** — application projects with business logic

### Module layers
Each module (Auth, Identity, etc.) follows a layered structure:
```
Module.Abstractions  → contracts, options, notification models
Module.Domain        → CQRS Commands/Queries, Handlers, Validators
Module.DataAccess    → EF entities, DbContext, configurations, migrations
Module.Application   → INotificationHandler<>, MassTransit consumers, orchestration
```

### Key projects
| Project | Purpose |
|---------|---------|
| `Avemepls.Core` | Base interfaces: IHasId, IHasName, IHasIsActive, IMapper, pagination |
| `Avemepls.Domain` | Generic CQRS handlers (Create/Update/Delete/List/GetById), FluentValidation pipeline |
| `Avemepls.DataAccess` | Entity base class, EF extensions, TransactionPipelineBehavior |
| `Avemepls.Security` | Permissions, roles, IPrincipalAccessor |
| `Avemepls.ServiceBus` | MassTransit wrapper with cross-domain mediator |
| `Avemepls.Auditor` | Audit via EF interceptors + MediatR notifications |
| `Schedia.Core` | AppOptions, application configuration |
| `Schedia.Domain.Core` | MediatR pipeline behaviors registration + Mapster |
| `Schedia.DataAccess` | SchediaDbContext (main), DbSets, migrations |

## Architectural Patterns

### CQRS (Vertical Slice)
Each operation is a static class with nested types:
```csharp
public static class CreateSomething
{
    public class Command : IRequest<Result> { ... }

    internal sealed class Handler(SchediaDbContext dbContext, ...)
        : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command command, CancellationToken ct) { ... }
    }

    public class Validator : ExtendedAbstractValidator<Command> { ... }
}
```

### Generic Base Handlers
The framework provides base handlers for CRUD:
- `CreateCommandHandler<TCommand, TContext, TEntity>`
- `UpdateCommandHandler<TCommand, TContext, TEntity>`
- `DeleteCommandHandler<TCommand, TContext, TEntity>`
- `ListQueryHandler<TQuery, TContext, TEntity, TDto>`
- `GetEntityByIdQueryHandler<TQuery, TContext, TEntity, TDto>`

Base handlers automatically apply `.WhereIsActive()` and `.WhereIsNotDeleted()` for entities implementing the corresponding interfaces.

### Pipeline Behaviors (MediatR)
1. `TransactionPipelineBehavior` — wraps requests marked with `[Transaction]` in a TransactionScope
2. `FluentValidationPipelineBehavior` — runs all IValidator<TRequest> before the handler

### IQueryableModifier
Cross-cutting filtering (multi-tenancy, user-scoped data) via `IQueryableModifier<TEntity>`, automatically applied in base handlers.

### DI Registration
- Modules register via static `ServiceCollectionExtensions`
- `ServiceScan.SourceGenerator` for auto-registration of handlers and validators
- No Repository pattern — direct DbContext access

## Database

- **PostgreSQL** with extensions: citext, uuid-ossp, pg_trgm
- **Snake_case** naming convention (`UseSnakeCaseNamingConvention()`)
- **Entity base**: `Entity` → `Entity<T>` → `IHasId<T>`
- Composable interfaces: `IHasName`, `IHasIsActive`, `IHasDateCreated`, `IHasDateUpdated`, `IHasDateDeleted`, `IHasSortOrder`, `IHasCode`, `IHasUserCreated`
- DbContext split into partial classes (separate files for DbSets and OnModelCreating)
- `EntityFrameworkCore.Projectables` for LINQ-compilable computed properties
- Registration: `IDbContextFactory<T>` with `ServiceLifetime.Scoped`

## Messaging
- **MassTransit** + **RabbitMQ** as transport
- Outbox pattern via SchediaDbContext (`service_bus` schema)
- Modules register consumers via `services.AddConsumers(assembly)`

## Localization
- Source generator `Avemepls.RsGenerator` produces strongly-typed accessors from `.rs` files in `Locale/en/`
- Controlled via `<RsLocalize>True</RsLocalize>` in `Directory.Build.props`

## Build & Dependencies
- Target: `net10.0`
- Nullable: enabled
- Analyzers: StyleCop, SonarAnalyzer, NSubstitute.Analyzers, FluentAssertions.Analyzers
- Central Package Management: `Directory.Packages.props`
