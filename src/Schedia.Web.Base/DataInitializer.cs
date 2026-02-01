using Avemepls.Core.DataAccess.ContextInitializing;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Schedia.Web.Base;

public static class DataInitializer
{
    public static WebApplication InitializeData(this WebApplication webApp)
    {
        using var scope = webApp.Services.CreateScope();
        var dataInitializers = scope.ServiceProvider.GetServices<IContextInitializer>();

        foreach (var dataInitializer in dataInitializers)
        {
            dataInitializer.Initialize();
        }

        return webApp;
    }
}