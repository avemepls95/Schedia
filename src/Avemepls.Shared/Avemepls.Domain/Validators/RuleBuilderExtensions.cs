using FluentValidation;

namespace Avemepls.Domain.Validators;

/// <summary>
/// Extensions for the <see cref="IRuleBuilder{T,TProperty}"/>.
/// </summary>
public static class RuleBuilderExtensions
{
    /// <summary>
    /// URL валидатор.
    /// </summary>
    public static IRuleBuilderOptions<T, TProperty> Url<T, TProperty>(this IRuleBuilder<T, TProperty> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.SetValidator(new UrlValidator<T, TProperty>());
    }

    /// <summary>
    /// Указывает, что свойство содержит только дату.
    /// </summary>
    public static IRuleBuilderOptions<T, DateTime?> Date<T>(this IRuleBuilder<T, DateTime?> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.SetValidator(new NullableDateValidator<T>());
    }

    /// <summary>
    /// Указывает, что свойство содержит только дату.
    /// </summary>
    public static IRuleBuilderOptions<T, DateTime> Date<T>(this IRuleBuilder<T, DateTime> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.SetValidator(new DateValidator<T>());
    }

    /// <summary>
    /// Устанавливает валидатор, который проверяет что элементы в коллекция являются уникальными.
    /// </summary>
    public static IRuleBuilder<T, TCollection> Unique<T, TCollection, TItem>(
        this IRuleBuilder<T, TCollection> builder)
        where TCollection : IEnumerable<TItem>
        => builder.Unique(EqualityComparer<TItem>.Default);

    /// <summary>
    /// Устанавливает валидатор, который проверяет что элементы в коллекции являются уникальными.
    /// </summary>
    public static IRuleBuilder<T, TCollection> Unique<T, TCollection, TItem>(
        this IRuleBuilder<T, TCollection> builder,
        IEqualityComparer<TItem> comparer)
        where TCollection : IEnumerable<TItem>
    {
        return builder.SetValidator(new UniqueCollectionValidator<T, TCollection, TItem>("{PropertyName} - значения должны быть уникальными.", comparer));
    }
}