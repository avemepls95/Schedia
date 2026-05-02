using Avemepls.Blazor;

using Microsoft.AspNetCore.Components;

using MudBlazor.Services;

using Schedia.Web.Blazor.Services;
using Schedia.Web.Core.Blazor.BlazorAssembliesRegistry;

namespace Schedia.Web.Blazor;

public static class ProgramExtensions
{
    public static void AddBlazor(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();

        // Register HttpClient for API calls
        builder.Services.AddScoped(sp => new HttpClient
        {
            BaseAddress = new Uri(sp.GetRequiredService<NavigationManager>().BaseUri)
        });

        builder.AddAuth();

        builder.Services.AddBlazorCore();
        builder.Services.AddBlazorPages(typeof(Schedia.Web.Base.Services.IPlatformService).Assembly);
        builder.Services.AddAntDesign();
        builder.Services.AddMudServices();
        builder.Services.AddSingleton<Schedia.Web.Base.Services.IPlatformService, WebPlatformService>();
        builder.Services.AddRazorPages();

        builder.Services
            .AddRazorComponents(options => options.DetailedErrors = builder.Environment.IsDevelopment())
            .AddInteractiveServerComponents();

        builder.WebHost.UseStaticWebAssets(); // for include blazor _content
    }
}