using FluentResults;
using Microsoft.Extensions.Logging;
using Muninn.Auction.Application.Contracts.Pipeline;
using Muninn.Auction.Application.Extensions;
using static System.Diagnostics.Stopwatch;

namespace Muninn.Auction.Application.Behaviors;

internal sealed class TimeTrackingBehavior<TRequest, TResult>(ILogger<IPipelineBehavior<TRequest, TResult>> logger) 
    : IPipelineBehavior<TRequest, TResult> where TRequest : IRequest<TResult>
{
    private readonly ILogger _logger = logger;
    private const string Success = "successfully";
    private const string Fail = "with failure";
    
    public async Task<Result<TResult>> HandleAsync(TRequest request, RequestHandleDelegate<TResult> next, CancellationToken cancellationToken = default)
    {
        var timestamp = GetTimestamp();
        var result = await next(cancellationToken);
        var elapsedTime = GetElapsedTime(timestamp);
        _logger.RequestFinished(typeof(TRequest).Name, elapsedTime, result.IsSuccess ? Success : Fail);
        
        return result;
    }
}
