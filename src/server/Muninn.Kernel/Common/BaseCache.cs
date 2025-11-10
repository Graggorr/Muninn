using Microsoft.Extensions.Logging;
using Muninn.Kernel.Extensions;
using Muninn.Kernel.Models;

namespace Muninn.Kernel.Common;

public abstract class BaseCache<TSelf>(ILogger<TSelf> logger, IFilterService filterService)
{
    internal const int DefaultIncreaseValue = 1000;
    internal const int InitialArraySize = 10_000;
    
    protected readonly ILogger _logger = logger;
    protected readonly IFilterService _filterService = filterService;
    protected readonly SemaphoreSlim _semaphoreSlim = new(1);

    protected MuninnResult GetCancelledResult(string methodName, string key, OperationCanceledException operationCanceledException)
    {
        _logger.LogCancelledRequest(methodName, key, operationCanceledException);

        return GetFailedResult("Cancellation has been requested", true, operationCanceledException);
    }

    protected MuninnResult GetCancelledResult(OperationCanceledException operationCanceledException)
    {
        _logger.LogCancelledRequest(operationCanceledException);

        return GetFailedResult("Cancellation has been requested", true, operationCanceledException);
    }
    
    protected static MuninnResult GetSuccessfulResult(Entry? entry = null) => new(true, entry);
    
    protected static MuninnResult GetFailedResult(string message, bool isCancelled, Exception? exception = null) => new(false, null, message, exception, isCancelled);
}
