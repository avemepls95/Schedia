using Avemepls.Auditor.Core.Interfaces;
using Avemepls.Auditor.Interfaces;

using MediatR;

namespace Avemepls.Auditor.Commands;

public class CommandProcessingAuditorBehavior<TCommand, TResponse>(
    IAuditScopeFactory auditScopeFactory,
    IEnumerable<ICommandProcessingAuditor<TCommand, TResponse>> processors,
    IEnumerable<ICommandProcessingAuditor<TCommand>> processorsNoResponse)
    : IPipelineBehavior<TCommand, TResponse>
    where TCommand : notnull
{
    private readonly ICollection<ICommandProcessingAuditor<TCommand, TResponse>> _processors = processors.ToList();

    public async Task<TResponse> Handle(
        TCommand request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is IAuditableCommand && (_processors.Any() || processorsNoResponse.Any()))
        {
            await using var scope = await auditScopeFactory.Create(null, cancellationToken);

            var processorsWithContext = _processors
                .Select(p => (Processor: p, Context: new CommandProcessingAuditorContext()))
                .ToArray();

            var processorsWithContextNoResponce = processorsNoResponse
                .Select(p => (Processor: p, Context: new CommandProcessingAuditorContext()))
                .ToArray();

            foreach (var processorWithContext in processorsWithContext)
            {
                await processorWithContext.Processor.PreProcess(
                    scope,
                    request,
                    processorWithContext.Context,
                    cancellationToken);
            }

            foreach (var processorWithContext in processorsWithContextNoResponce)
            {
                await processorWithContext.Processor.PreProcess(
                    scope,
                    request,
                    processorWithContext.Context,
                    cancellationToken);
            }

            var result = await next(cancellationToken);

            foreach (var processorWithContext in processorsWithContextNoResponce)
            {
                await processorWithContext.Processor.PostProcess(
                    scope,
                    request,
                    processorWithContext.Context,
                    cancellationToken);
            }

            foreach (var processorWithContext in processorsWithContext)
            {
                await processorWithContext.Processor.PostProcess(
                    scope,
                    request,
                    result,
                    processorWithContext.Context,
                    cancellationToken);
            }

            return result;
        }

        return await next(cancellationToken);
    }
}