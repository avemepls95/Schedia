using Avemepls.Infrastructure;
using Elastic.Ingest.Elasticsearch;
using Elastic.Serilog.Sinks;
using Microsoft.AspNetCore.Localization;
using Microsoft.IdentityModel.Logging;
using Schedia.Web.Base;
using Schedia.Web.Blazor;
using Schedia.Web.Blazor.Components;
using Schedia.Web.Blazor.Endpoints;
using Schedia.Web.Core.Blazor.BlazorAssembliesRegistry;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    var services = builder.Services;

    var elasticsearchNodes = builder.Configuration
        .GetSection("Elasticsearch:Nodes")
        .Get<string[]>() ?? ["http://localhost:9200"];

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .WriteTo.Elasticsearch(
            elasticsearchNodes.Select(n => new Uri(n)).ToArray(),
            opts => opts.BootstrapMethod = BootstrapMethod.Silent));

    // Add services to the container.
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    var configuration = builder.Configuration;

    services
        .AddMemoryCache()
        .AddInfrastructureServices();

    builder.Services.AddSchediaBase(configuration);
    builder.AddBlazor();

    var app = builder
        .Build()
        .InitializeData();

    app.UseSerilogRequestLogging();
    app.UseForwardedHeaders();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    if (!app.Environment.IsDevelopment())
    {
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
        app.UseHttpsRedirection();
    }
    else
    {
        IdentityModelEventSource.ShowPII = true;
    }

    app.UseDeveloperExceptionPage();
    app.UseStaticFiles();
    app.UseRouting();

    var localizationOptions = new RequestLocalizationOptions()
        .SetDefaultCulture("en")
        .AddSupportedCultures("ru", "en")
        .AddSupportedUICultures("ru", "en");

    localizationOptions.RequestCultureProviders.Insert(0,
        new CookieRequestCultureProvider { CookieName = "Schedia.Culture" });

    app.UseRequestLocalization(localizationOptions);

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseAntiforgery();

    if (!app.Environment.IsDevelopment())
    {
        app.UseResponseCompression();
    }

    app.MapControllers();
    app.MapAuthEndpoints();

    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode()
        .AddAdditionalAssemblies([.. BlazorAssembliesRegistry.Assemblies]);

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}