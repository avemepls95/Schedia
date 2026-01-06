namespace Avemepls.Blazor.Common.Menus;

/// <summary>
/// Menu sections builder
/// </summary>
public class MenuSectionsBuilder
{
    private readonly List<MenuItemModel> _menuItems = [];

    /// <summary>
    /// All menu items registered in app
    /// </summary>
    public IEnumerable<MenuItemModel> MenuItems => _menuItems;

    /// <summary>
    /// Sets the menuitem of menu, which usually represents in menu's tab which routes to URL on click
    /// </summary>
    public MenuSectionsBuilder WithMenuItem(string key, string title, string url, string? permission)
    {
        _menuItems.Add(new MenuItemModel(key, url, title, permission));
        return this;
    }

    /// <summary>
    /// Sets the section of menu, which usually represents in submenu for menu items that some module provides
    /// </summary>
    public MenuSectionsBuilder WithSection(string key, string title, params MenuItemModel[] items)
    {
        _menuItems.Add(new MenuItemModel(key, title, null, items));
        return this;
    }

    /// <summary>
    /// Sets the icon of section or menuitem
    /// </summary>
    public MenuSectionsBuilder WithIcon(string icon)
    {
        _menuItems[^1].Icon = icon;
        return this;
    }
}