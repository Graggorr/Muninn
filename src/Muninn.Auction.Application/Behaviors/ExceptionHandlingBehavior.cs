using FluentResults;
using Microsoft.Extensions.Logging;
using Muninn.Auction.Application.Contracts.Pipeline;
using Muninn.Auction.Application.Extensions;
using static System.Text.Json.JsonSerializer;
using static FluentResults.Result;

namespace Muninn.Auction.Application.Behaviors;

internal sealed class ExceptionHandlingBehavior<TRequest, TResult>(ILogger<IPipelineBehavior<TRequest, TResult>> logger)
    : IPipelineBehavior<TRequest, TResult> where TRequest : IRequest<TResult>
{
    private readonly ILogger _logger = logger;
    
    public async Task<Result<TResult>> HandleAsync(TRequest request, RequestHandleDelegate<TResult> next,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await next(cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            var serializedRequest = Serialize(request, typeof(TRequest));
            _logger.RequestError(typeof(TRequest).Name, serializedRequest, exception);
            
            return Fail<TResult>(exception.Message);
        }
    }
}
