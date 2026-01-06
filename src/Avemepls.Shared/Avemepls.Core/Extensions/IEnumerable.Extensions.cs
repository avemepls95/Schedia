namespace Avemepls.Core.Extensions;

public static class EnumerableExtensions
{
    /// <summary>
    /// Calculate order independent hash code via EqualityComparer.Default
    /// </summary>
    /// <param name="source">Enumerable of T</param>
    /// <typeparam name="T">Type for hashCode</typeparam>
    /// <returns>HashCode</returns>
    public static int GetOrderIndependentHashCode<T>(this IEnumerable<T> source)
        where T : notnull
    {
        var hash = 0;

        foreach (var element in source)
        {
            hash ^= EqualityComparer<T>.Default.GetHashCode(element);
        }

        return hash;
    }

    public static bool IsEmpty<T>(this IEnumerable<T> source) => !source.Any();

    /// <summary>
    /// Returns empty collection if arg is null
    /// </summary>
    public static IEnumerable<TModel> EmptyIfNull<TModel>(this IEnumerable<TModel>? collection)
    {
        return collection ?? [];
    }

    /// <summary>
    /// Действие для всех элементов
    /// </summary>
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
        {
            action(item);

            yield return item;
        }
    }

    /// <summary>
    /// Обновление всех элементов
    /// </summary>
    public static void Update<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
        {
            action(item);
        }
    }

    /// <summary>
    /// Преобразование не пустых строк в строку
    /// </summary>
    public static string JoinNonEmptyString(this IEnumerable<string> items, string separator)
    {
        return string.Join(separator, items.Where(x => !string.IsNullOrWhiteSpace(x)));
    }

    /// <summary>
    /// Проверка отсутсвия элемена
    /// </summary>
    public static bool NotExists<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        return !source.Any(predicate);
    }
}