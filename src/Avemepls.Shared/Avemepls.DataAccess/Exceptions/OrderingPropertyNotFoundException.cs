namespace Avemepls.Core.DataAccess.Exceptions;

/// <summary>
/// Исключение, возникающее при попытке сортировки коллекции по несуществующему свойству
/// </summary>
public class OrderingPropertyNotFoundException(Type ownerType, string propertyName)
    : Exception($"No property with path '{propertyName}' found in model of type '{ownerType.Name}'")
{
    /// <summary>
    /// Тип объекта
    /// </summary>
    public Type OwnerType { get; } = ownerType;

    /// <summary>
    /// Название свойства
    /// </summary>
    public string PropertyName { get; } = propertyName;
}