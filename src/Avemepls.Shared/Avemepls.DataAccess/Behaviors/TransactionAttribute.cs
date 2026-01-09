using System.Transactions;

namespace Avemepls.Core.DataAccess.Behaviors;

/// <summary>
/// Атрибут означающий, что выполнение всей команды будет осуществляться в рамках транзакции
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class TransactionAttribute : Attribute
{
    /// <summary>
    /// Уровень изоляции транзакции
    /// </summary>
    public IsolationLevel IsolationLevel { get; set; } = IsolationLevel.ReadCommitted;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionAttribute"/>.
    /// </summary>
    public TransactionAttribute()
    {
    }

    public TransactionAttribute(IsolationLevel isolationLevel)
    {
        IsolationLevel = isolationLevel;
    }
}