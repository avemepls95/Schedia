namespace Avemepls.Blazor.Common.Menus;

/// <summary>
/// Menu item
/// </summary>
public class MenuItemModel
{
    /// <summary>
    /// Order
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Key
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Icon
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Navigation URL
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Friendly title
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Required permission
    /// </summary>
    public string? Permission { get; set; }

    /// <summary>
    /// Subitems
    /// </summary>
    public MenuItemModel[]? Children { get; set; }

    public MenuItemModel(string key, string url, string title, string? permission = null)
    {
        Key = key;
        Url = url;
        Title = title;
        Permission = permission;
    }

    public MenuItemModel(string key, string title, string? permission = null, params MenuItemModel[] children)
    {
        Key = key;
        Title = title;
        Children = children;
        Permission = permission;
    }
}