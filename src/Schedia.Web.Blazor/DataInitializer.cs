using Avemepls.Core.DataAccess.ContextInitializing;

namespace Schedia.Web.Blazor;

public static class DataInitializer
{
    public static WebApplication InitializeData(this WebApplication webApp)
    {
        using var scope = webApp.Services.CreateScope();
        using var dataInitializer = scope.ServiceProvider.GetRequiredService<IContextInitializer>();
        dataInitializer.Initialize();

        return webApp;
    }
}