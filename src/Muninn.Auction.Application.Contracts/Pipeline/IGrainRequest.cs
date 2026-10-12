namespace Muninn.Auction.Application.Contracts.Pipeline;

public interface IGrainRequest<out TResult> : IRequest<TResult>
{
    Guid GrainId { get; }
}

public interface IGrainRequest<out TResult, TGrain> : IGrainRequest<TResult> where TGrain : IGrainWithGuidKey;
