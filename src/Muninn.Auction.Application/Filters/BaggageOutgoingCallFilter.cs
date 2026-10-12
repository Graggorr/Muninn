using OpenTelemetry;
using static Muninn.Auction.Application.Contracts.Constants;

namespace Muninn.Auction.Application.Filters;

internal sealed class BaggageOutgoingCallFilter : IOutgoingGrainCallFilter
{
    public Task Invoke(IOutgoingGrainCallContext context)
    {
        foreach (var key in PropagatedContextKeys)
        {
            if (Baggage.GetBaggage(key) is { } value)
            {
                RequestContext.Set(key, value);
            }
        }

        return context.Invoke();
    }
}
