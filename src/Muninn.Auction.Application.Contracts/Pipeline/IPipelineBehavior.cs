using FluentResults;

namespace Muninn.Auction.Application.Contracts.Pipeline;

public delegate Task<Result<TResult>> RequestHandleDelegate<TResult>(CancellationToken cancellationToken = default);

public delegate Task<Result> RequestHandleDelegate(CancellationToken cancellationToken = default);

public interface IPipelineBehavior<in TRequest, TResult> where TRequest : IRequest<TResult>
{
    Task<Result<TResult>> HandleAsync(TRequest request, RequestHandleDelegate<TResult> next, CancellationToken cancellationToken = default);
}

public interface IPipelineBehavior<in TRequest> where TRequest : IRequest
{
    Task<Result> HandleAsync(TRequest request, RequestHandleDelegate next, CancellationToken cancellationToken = default);
}
