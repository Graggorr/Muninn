using FluentResults;

namespace Muninn.Auction.Application.Contracts.Pipeline;

public interface IMediator
{
    Task<Result<TResult>> SendAsync<TRequest, TResult, TGrain>(TRequest request, CancellationToken cancellationToken = default) 
        where TRequest : IGrainRequest<TResult, TGrain> where TGrain : IGrainWithGuidKey;

    Task<Result<TResult>> SendAsync<TRequest, TResult>(TRequest request, CancellationToken cancellationToken = default) 
        where TRequest : IRequest<TResult>;
    
    Task<Result> PublishAsync<TEvent, TGrain>(TEvent grainEvent, CancellationToken cancellationToken = default) 
        where TEvent : IGrainEvent<TGrain> where TGrain : IGrainWithGuidKey;

    Task<Result> PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent;
}
