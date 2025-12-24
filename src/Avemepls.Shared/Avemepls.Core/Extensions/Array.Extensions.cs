namespace Avemepls.Core.Extensions;

public static class ArrayExtensions
{
    /// <summary>
    /// Проверка на существование
    /// </summary>
    public static bool Exists<T>(this T[] array, Predicate<T> predicate)
    {
        return Array.Exists(array, predicate);
    }

    /// <summary>
    /// Проверка на несуществование
    /// </summary>
    public static bool NotExists<T>(this T[] array, Predicate<T> predicate)
    {
        return !Array.Exists(array, predicate);
    }

    /// <summary>
    /// Проверка на удовлетворение всех элементов условию
    /// </summary>
    public static bool TrueForAll<T>(this T[] array, Predicate<T> predicate)
    {
        return Array.TrueForAll(array, predicate);
    }

    /// <summary>
    /// Поиск элемента по условию
    /// </summary>
    public static T? Find<T>(this T[] array, Predicate<T> predicate)
    {
        return Array.Find(array, predicate);
    }
}