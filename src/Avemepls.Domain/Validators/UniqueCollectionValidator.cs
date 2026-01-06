using FluentValidation;
using FluentValidation.Validators;

namespace Avemepls.Domain.Validators;

/// <summary>
/// Валидатор для проверки, что коллекция содержит только уникальные значения.
/// </summary>
public class UniqueCollectionValidator<T, TCollection, TItemType> : PropertyValidator<T, TCollection>
    where TCollection : IEnumerable<TItemType>
{
    private readonly string _errorMessage;
    private readonly IEqualityComparer<TItemType> _comparer;

    /// <summary>
    /// Initializes a new instance
    /// </summary>
    public UniqueCollectionValidator(string errorMessage, IEqualityComparer<TItemType> comparer)
    {
        _errorMessage = errorMessage;
        _comparer = comparer;
    }

    /// <inheritdoc />
    public override bool IsValid(ValidationContext<T> context, TCollection value)
    {
        var enumerable = value as TItemType[] ?? value.ToArray();
        return enumerable.Distinct(_comparer).Count() == enumerable.Length;
    }

    public override string Name => "UniqueCollectionValidator";

    protected override string GetDefaultMessageTemplate(string errorCode)
    {
        return _errorMessage;
    }
}