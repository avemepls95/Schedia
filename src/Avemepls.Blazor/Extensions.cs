using Avemepls.Blazor.Common.Menus;
using Avemepls.Blazor.Common.Menus.Providers;
using Avemepls.Blazor.Filters;
using Avemepls.Blazor.MediatR;
using Avemepls.Blazor.Navigation;
using Avemepls.Mvc.StateProviders.Filter;

using Microsoft.Extensions.DependencyInjection;

namespace Avemepls.Blazor;

public static class Extensions
{
    public static IServiceCollection AddBlazorCore(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<IFilterStateProvider, QueryStringStateProvider>();
        serviceCollection.AddScoped<NavigationHistoryManager>();
        serviceCollection.AddScopedMediator();

        return serviceCollection;
    }

    public static IServiceCollection AddMenuSections(this IServiceCollection services, Action<MenuSectionsBuilder> builder)
    {
        var sectionBuilder = new MenuSectionsBuilder();
        builder(sectionBuilder);
        services.AddSingleton<IMenuSectionsProvider>(new FuncMenuSectionsProvider(() => sectionBuilder.MenuItems));
        return services;
    }
}