using System.Reflection;

using Avemepls.Infrastructure.DateTime;

using Schedia.Web.MAUI.Services;
using Schedia.Web.Shared.Services;

namespace Schedia.Web.MAUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // Загрузить appsettings.json из embedded resource
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("Schedia.Web.MAUI.appsettings.json");

        var config = new ConfigurationBuilder()
            .AddJsonStream(stream!)
            .Build();

        builder.Configuration.AddConfiguration(config);

        // MudBlazor
        builder.Services.AddMudServices();

        // MediatR - регистрировать Domain handlers
        builder.Services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(typeof(Schedia.Auth.Domain.ServiceCollectionExtensions).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(Schedia.Identity.Domain.User.ConfirmEmail).Assembly);
        });

        // Entity Framework Core + PostgreSQL
        builder.Services.AddDataAccess((_, cfg) =>
        {
            cfg.UseNpgsql(
                config.GetConnectionString("Schedia"),
                o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
            )
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        // Infrastructure services
        builder.Services
            .AddMemoryCache()
            .AddSingleton<ICurrentDateTimeProvider, CurrentSystemDateTimeProvider>()
            .AddScoped<IPrincipalAccessor, MauiPrincipalAccessor>();

        // Platform service
        builder.Services.AddSingleton<IPlatformService, MauiPlatformService>();

        // Authentication
        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddScoped<MauiAuthenticationStateProvider>();
        builder.Services.AddScoped<AuthenticationStateProvider>(
            sp => sp.GetRequiredService<MauiAuthenticationStateProvider>()
        );
        builder.Services.AddScoped<IAuthStorageService, MauiAuthStorageService>();
        builder.Services.AddScoped<IAuthenticationService, MauiAuthenticationService>();

        // Domain modules (используют extension methods)
        var configuration = builder.Configuration;
        builder.Services.AddSchediaAuth(configuration);
        builder.Services.AddSchediaIdentity();

        return builder.Build();
    }
}
