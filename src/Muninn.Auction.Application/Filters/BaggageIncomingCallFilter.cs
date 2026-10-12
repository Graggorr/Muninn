using OpenTelemetry;
using static Muninn.Auction.Application.Contracts.Constants;

namespace Muninn.Auction.Application.Filters;

internal sealed class BaggageIncomingCallFilter : IIncomingGrainCallFilter
{
    public Task Invoke(IIncomingGrainCallContext context)
    {
        foreach (var key in PropagatedContextKeys)
        {
            if (RequestContext.Get(key) is string value)
            {
                Baggage.SetBaggage(key, value);
            }
        }

        return context.Invoke();
    }
}
