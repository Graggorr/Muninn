namespace Muninn.Auction.Application.Contracts.Pipeline;

public interface IEvent : IRequest
{
    Guid EventId { get; init; }
}

public interface IGrainEvent<TGrain> : IEvent where TGrain : IGrainWithGuidKey
{
    Guid GrainId { get; init; }
}
