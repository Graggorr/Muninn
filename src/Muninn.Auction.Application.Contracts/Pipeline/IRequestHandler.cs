using FluentResults;

namespace Muninn.Auction.Application.Contracts.Pipeline;

public interface IRequestHandler<in TRequest, TResult> where TRequest : IRequest<TResult>
{
    Task<Result<TResult>> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}
