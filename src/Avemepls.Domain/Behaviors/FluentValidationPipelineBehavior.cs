using FluentValidation;

using MediatR;

namespace Avemepls.Domain.Behaviors;

public class FluentValidationPipelineBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        return HandleInternal(request, next, cancellationToken);
    }

    internal async Task<TResponse> HandleInternal(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);
        var validationResultTasks = validators
            .Select(async v => await v.ValidateAsync(context, cancellationToken).ConfigureAwait(false));

        var validationResults = await Task.WhenAll(validationResultTasks).ConfigureAwait(false);

        var failures = validationResults.FirstOrDefault()?
            .Errors
            .Where(f => f != null)
            .ToList();

        if (failures?.Count > 0)
        {
            throw new ValidationException(failures);
        }

        return await next!(cancellationToken).ConfigureAwait(false);
    }
}