using Avemepls.Core.Models;

using MassTransit;

namespace Avemepls.ServiceBus.Filters;

/// <summary>
/// Add header <see cref="ServiceBusConstants.HeaderNames.Context"/> to event if <see cref="ScopeContext.Name"/> of
/// <see cref="ScopeContext"/> specified.
/// </summary>
internal class ScopeContextPublishFilter : IFilter<PublishContext>
{
    public void Probe(ProbeContext context) => context.CreateFilterScope("ScopeContext");

    public Task Send(PublishContext context, IPipe<PublishContext> next)
    {
        if (!string.IsNullOrWhiteSpace(ScopeContext.Name))
        {
            context.Headers.Set(ServiceBusConstants.HeaderNames.Context, ScopeContext.Name);
        }

        return next.Send(context);
    }
}