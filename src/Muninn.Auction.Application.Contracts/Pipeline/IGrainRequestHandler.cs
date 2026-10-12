using FluentResults;

namespace Muninn.Auction.Application.Contracts.Pipeline;

public interface IGrainRequestHandler<in TRequest, TResult, in TGrain> where TRequest : IGrainRequest<TResult, TGrain>
    where TGrain : IGrainWithGuidKey
{
    Task<Result<TResult>> HandleAsync(TRequest request, TGrain grain, CancellationToken cancellationToken = default);
}
