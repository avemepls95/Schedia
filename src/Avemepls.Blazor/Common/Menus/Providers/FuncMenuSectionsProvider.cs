namespace Avemepls.Blazor.Common.Menus.Providers;

public class FuncMenuSectionsProvider(Func<IEnumerable<MenuItemModel>> builder) : IMenuSectionsProvider
{
    public IEnumerable<MenuItemModel> GetMenuItems()
    {
        return builder();
    }
}