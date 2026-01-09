using System.Reflection;
using System.Transactions;

using MediatR;

using Microsoft.Extensions.Logging;

namespace Avemepls.Core.DataAccess.Behaviors;

/// <summary>
/// Обеспечивает выполнение команд, отмеченных атрибутом <see cref="TransactionAttribute"/> в рамках транзакции
/// </summary>
public class TransactionPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<TransactionPipelineBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionPipelineBehavior{TRequest, TResponse}"/>.
    /// </summary>
    public TransactionPipelineBehavior(ILogger<TransactionPipelineBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var transaction = typeof(TRequest).GetCustomAttribute<TransactionAttribute>();

        if (transaction == null)
        {
            return await next(cancellationToken);
        }

        var options = GetTransactionOptions(transaction);

        _logger.LogDebug(
            "Request {RequestType} is running inside transaction with isolation level {Level}",
            typeof(TRequest).FullName,
            options.IsolationLevel);

        using var tran = new TransactionScope(TransactionScopeOption.Required, options, TransactionScopeAsyncFlowOption.Enabled);
        var result = await next(cancellationToken);
        tran.Complete();

        return result;
    }

    private static TransactionOptions GetTransactionOptions(TransactionAttribute transaction)
    {
        var opts = new TransactionOptions
        {
            IsolationLevel = transaction.IsolationLevel
        };

        return opts;
    }
}