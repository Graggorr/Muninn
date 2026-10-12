using FluentResults;

namespace Muninn.Auction.Application.Contracts.Pipeline;

public interface IGrainEventHandler<in TEvent, in TGrain> where TEvent : IGrainEvent<TGrain> where TGrain : IGrainWithGuidKey
{
    Task<Result> HandleAsync(TEvent grainEvent, TGrain grain, CancellationToken cancellationToken);
}
