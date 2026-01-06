using AntDesign;
using AntDesign.TableModels;

using Avemepls.Core.Models;

namespace Avemepls.Blazor.Helpers;

public static class ComponentsHelpers
{
    /// <summary>
    /// Получение стиля для записи справочника
    /// </summary>
    /// <typeparam name="T">Тип записи</typeparam>
    /// <param name="row">Запись справочника</param>
    /// <returns>Css класс</returns>
    public static string GetClassForDictionaryEntityRecord<T>(RowData<T> row) => GetClass(row.Data!);

    /// <summary>
    ///  Получение стиля для записи древовидного справочника
    /// </summary>
    /// <typeparam name="T">Тип записи</typeparam>
    /// <param name="node">Запись справочника</param>
    /// <returns>Css класс</returns>
    public static string GetClassForTreeDictionaryEntityRecord<T>(TreeNode<T> node) => GetClass(node.DataItem!);

    private static string GetClass(object entity)
    {
        var rowClass = string.Empty;

        if (entity is IHasDateDeleted hasDate && hasDate.DateDeleted is not null)
            rowClass += BlazorGlobalConfiguration.DeletedStyle;

        if (entity is IHasIsActive hasActive && !hasActive.IsActive)
            rowClass += BlazorGlobalConfiguration.DeactivatedStyle;

        return rowClass;
    }
}