namespace Avemepls.Blazor.Common.Menus.Providers;

public interface IMenuSectionsProvider
{
    IEnumerable<MenuItemModel> GetMenuItems();
}