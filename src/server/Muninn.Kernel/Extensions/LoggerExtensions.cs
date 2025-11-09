using Microsoft.Extensions.Logging;

namespace Muninn.Kernel.Extensions;

internal static partial class LoggerExtensions
{
    [LoggerMessage(EventId = 8001, Level = LogLevel.Error, Message = "Cannot insert file with name {Name}", SkipEnabledCheck = true)]
    public static partial void LogFailedFileInsert(this ILogger logger, string name, Exception exception);

    [LoggerMessage(EventId = 8002, Level = LogLevel.Error, Message = "Cannot delete file with name {Name}", SkipEnabledCheck = true)]
    public static partial void LogFailedFileDelete(this ILogger logger, string name, Exception exception);

    [LoggerMessage(EventId = 8003, Level = LogLevel.Error, Message = "Cannot create a stream for {Operation} of file {Name}", SkipEnabledCheck = true)]
    public static partial void LogFailedStreamCreate(this ILogger logger, string operation, string name, Exception exception);

    [LoggerMessage(EventId = 8004, Level = LogLevel.Error, Message = "Cannot add new value for key {Key}", SkipEnabledCheck = true)]
    public static partial void LogFailedKeyInsert(this ILogger logger, string key, Exception exception);

    [LoggerMessage(EventId = 8005, Level = LogLevel.Error, Message = "Cannot update value for key {Key} with new value", SkipEnabledCheck = true)] 
    public static partial void LogFailedKeyUpdate(this ILogger logger, string key, Exception exception);

    [LoggerMessage(EventId = 8006, Level = LogLevel.Error, Message = "Cannot read the file {Name}", SkipEnabledCheck = true)]
    public static partial void LogFailedFileRead(this ILogger logger, string name, Exception exception);

    [LoggerMessage(EventId = 8007, Level = LogLevel.Warning, Message = "Request {Request} for {Key} has been cancelled", SkipEnabledCheck = true)]
    public static partial void LogCancelledRequest(this ILogger logger, string request, string key, OperationCanceledException? operationCanceledException = null);

    [LoggerMessage(EventId = 8008, Level = LogLevel.Debug, Message = "Key {Key} has been added")]
    public static partial void LogKeyAdd(this ILogger logger, string key);

    [LoggerMessage(EventId = 8009, Level = LogLevel.Debug, Message = "Key {Key} has been updated. New value:\n{value}")]
    public static partial void LogKeyUpdate(this ILogger logger, string key, byte[] value);

    [LoggerMessage(EventId = 8010, Level = LogLevel.Debug, Message = "Key {Key} has been deleted")]
    public static partial void LogKeyDelete(this ILogger logger, string key);

    [LoggerMessage(EventId = 8011, Level = LogLevel.Debug, Message = "File {Name} has been inserted")]
    public static partial void LogFileInsert(this ILogger logger, string name);

    [LoggerMessage(EventId = 8012, Level = LogLevel.Debug, Message = "File {Name} has been deleted")]
    public static partial void LogFileDelete(this ILogger logger, string name);

    [LoggerMessage(EventId = 8013, Level = LogLevel.Information, Message = "Key {Key} is not found")]
    public static partial void LogKeyIsNotFound(this ILogger logger, string key);

    [LoggerMessage(EventId = 8014, Level = LogLevel.Error, Message = "Cannot sort entries", SkipEnabledCheck = true)]
    public static partial void LogSortError(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 8014, Level = LogLevel.Warning, Message = "Cache optimization takes a lot of time: {ElapsedTime}")]
    public static partial void LogSlowSorting(this ILogger logger, TimeSpan elapsedTime);

    [LoggerMessage(EventId = 8015, Level = LogLevel.Error, Message = "Cannot delete expired data for key {Key}", SkipEnabledCheck = true)]
    public static partial void LogExpiredKeyDeleteError(this ILogger logger, string key, Exception? exception = null);

    [LoggerMessage(EventId = 8016, Level = LogLevel.Warning, Message = "Request has been cancelled", SkipEnabledCheck = true)]
    public static partial void LogCancelledRequest(this ILogger logger, OperationCanceledException operationCanceledException);

    [LoggerMessage(EventId = 8017, Level = LogLevel.Error, Message = "Cannot delete key with {Key}", SkipEnabledCheck = true)]
    public static partial void LogFailedKeyDelete(this ILogger logger, string key, Exception exception);

    [LoggerMessage(EventId = 8018, Level = LogLevel.Debug, Message = "Array size has been increased. Current size is {Size}")]
    public static partial void LogIncreasedSize(this ILogger logger, int size);
    
    [LoggerMessage(EventId = 8019, Level = LogLevel.Debug, Message = "Array size has been decreased. Current size is {Size}")]
    public static partial void LogDecreasedSize(this ILogger logger, int size);
    
    [LoggerMessage(EventId = 8020, Level = LogLevel.Warning, Message = "Request {Request} has been cancelled", SkipEnabledCheck = true)]
    public static partial void LogCancelledRequest(this ILogger logger, string request, OperationCanceledException operationCanceledException);

    [LoggerMessage(EventId = 8021, Level = LogLevel.Error, Message = "Cannot clear cache", SkipEnabledCheck = true)]
    public static partial void LogClearAsyncError(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 8021, Level = LogLevel.Error, Message = "Cannot increase array size", SkipEnabledCheck = true)]
    public static partial void LogIncreaseArraySizeAsyncError(this ILogger logger, Exception exception);
    
    [LoggerMessage(EventId = 8022, Level = LogLevel.Error, Message = "Cannot decrease array size", SkipEnabledCheck = true)]
    public static partial void LogDecreaseArraySizeAsyncError(this ILogger logger, Exception exception);
    
    [LoggerMessage(EventId = 8023, Level = LogLevel.Error, Message = "Error has raised during the finding {Key}", SkipEnabledCheck = true)]
    public static partial void LogGetError(this ILogger logger, string key, Exception exception);
}
