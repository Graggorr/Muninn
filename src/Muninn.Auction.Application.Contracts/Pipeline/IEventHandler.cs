using FluentResults;

namespace Muninn.Auction.Application.Contracts.Pipeline;

public interface IEventHandler<in TEvent> where TEvent : IEvent
{
    Task<Result> HandleAsync(TEvent @event, CancellationToken cancellationToken);
}
