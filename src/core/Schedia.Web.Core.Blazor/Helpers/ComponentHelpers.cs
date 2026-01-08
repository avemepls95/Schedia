using AntDesign.TableModels;

using Avemepls.Core.Models;

namespace Schedia.Web.Core.Blazor.Helpers;

public static class ComponentHelpers
{
    public static string GetRowMatchClass<T>(T data, string? query)
        where T : DictionaryModelSlim
    {
        return !string.IsNullOrEmpty(query) && data.Name.Contains(query)
            ? "search-match "
            : string.Empty;
    }

    public static string GetStyleElementRowClass<T>(RowData<T> row)
        where T : DictionaryModelSlim
    {
        var rowClasses = new List<string>();

        if (row.Data is IHasDateDeleted { DateDeleted: not null })
        {
            rowClasses.Add("deleted");
        }

        if (row.Data is IHasIsActive { IsActive: false })
        {
            rowClasses.Add("row-deactivated");
        }

        return string.Join(" ", rowClasses);
    }
}