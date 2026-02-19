# Schedia UI — Blazor / MAUI

## UI Architecture

Three projects form the UI layer:
- **Schedia.Web.Base** (this project) — shared UI: Razor pages, layouts, services, theme. Razor Class Library, platform-independent
- **Schedia.Web.Blazor** — web server hosting (ASP.NET Core, cookie authentication)
- **Schedia.Web.MAUI** — mobile hosting (iOS + Android, JWT in secure storage)

All platform-independent code goes into `Schedia.Web.Base`. Platform-specific implementations only go into their respective hosting projects.

## Platform Abstractions

Interfaces are declared in `Schedia.Web.Base`, implementations live in platform projects:

| Interface | Blazor (Web) | MAUI |
|-----------|-------------|------|
| `IAuthenticationService` | `WebAuthenticationService` — cookie via JS interop | `MauiAuthenticationService` — JWT in secure storage |
| `IPlatformService` | `WebPlatformService` | `MauiPlatformService` |
| `IAuthStorageService` | not used | `MauiAuthStorageService` |
| `IPrincipalAccessor` | `BlazorPrincipalAccessor` (from HttpContext) | `MauiPrincipalAccessor` (from JWT) |
| `AuthenticationStateProvider` | `TokenAuthenticationStateProvider` | `MauiAuthenticationStateProvider` |

## Routing

`BlazorAssembliesRegistry` — static assembly registry. Pages from `Schedia.Web.Base` are registered via `AddBlazorPages(assembly)`, then connected:
```csharp
app.MapRazorComponents<App>()
    .AddAdditionalAssemblies([.. BlazorAssembliesRegistry.Assemblies]);
```

## Authentication

- **Blazor (Web)**: Cookie-based. JWT is only used for initial token parsing. Cookie is set via auth endpoints (`/auth/login`, `/auth/logout` in `AuthEndpoints.cs`)
- **MAUI**: Pure JWT flow. Tokens are stored in device secure storage, auth state is reconstituted from JWT on app launch

## UI Libraries

- **MudBlazor** — primary component library (both platforms)
- **AntDesign** — used in `Avemepls.Blazor` (framework base components)
- `SchediaTheme` — custom MudBlazor theme (`SchediaTheme.cs`)

## Layouts
- `AnonymousLayout` — for unauthenticated pages (login, register)
- `AppLayout` — main application layout
- `AdministrationLayout` — admin panel

## UI DI Registration
Shared registration via `Program.Extensions.cs`:
```csharp
services.AddSchediaBase(configuration);
```
Platform-specific implementations are registered in each hosting project's `Program.cs`.
