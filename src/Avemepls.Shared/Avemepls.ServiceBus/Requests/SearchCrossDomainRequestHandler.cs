using Avemepls.Core.Mapping;
using Avemepls.Core.Models;
using Avemepls.Domain.Queries;
using Avemepls.ServiceBus.Exceptions;

using MassTransit;

using MediatR;

namespace Avemepls.ServiceBus.Requests;

public class SearchCrossDomainRequestHandler<
    TCrossDomainRequest,
    TDomainRequest,
    TCrossDomainModel,
    TDomainModel,
    TModel>(ISender mediator, IMapper mapper) :
    CrossDomainRequestHandlerBase<TCrossDomainRequest>
    where TCrossDomainRequest : class, ISearchQuery<TModel>, ICrossDomainRequest<PagedResponse<TCrossDomainModel>>, new()
    where TDomainRequest : SearchQuery<TDomainModel>
    where TDomainModel : class, TCrossDomainModel
    where TModel : class, TCrossDomainModel
{
    protected override async ValueTask<object?> Execute(
        TCrossDomainRequest message,
        CancellationToken cancellationToken)
    {
        var request = MapRequest(message);

        var result = await mediator.Send(request, cancellationToken);

        return new PagedResponse<TCrossDomainModel>(result.Results.Cast<TCrossDomainModel>().ToArray(), result.Count);
    }

    protected virtual TDomainRequest MapRequest(TCrossDomainRequest request)
    {
        return mapper.Map<TDomainRequest>(request);
    }

    protected override async Task HandleResult(ConsumeContext<TCrossDomainRequest> context, object? result)
    {
        if (result is null)
        {
            await context.RespondAsync(new CrossDomainRequestException("No result received"));
        }

        await base.HandleResult(context, result);
    }
}