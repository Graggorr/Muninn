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

    [LoggerMessage(EventId = 8004, Level = LogLevel.Error, Message = "Cannot add new value for key {Key} with value {Value}", SkipEnabledCheck = true)]
    public static partial void LogFailedKeyInsert(this ILogger logger, string key, string value, Exception exception);

    [LoggerMessage(EventId = 8005, Level = LogLevel.Error, Message = "Cannot update value for key {Key} with new value {Value}", SkipEnabledCheck = true)] 
    public static partial void LogFailedKeyUpdate(this ILogger logger, string key, string value, Exception exception);

    [LoggerMessage(EventId = 8006, Level = LogLevel.Error, Message = "Cannot read the file {Name}")]
    public static partial void LogFailedFileRead(this ILogger logger, string name, Exception exception);

    [LoggerMessage(EventId = 8007, Level = LogLevel.Warning, Message = "Request for key {Key} has been cancelled")]
    public static partial void LogCancelledRequest(this ILogger logger, string key, OperationCanceledException? operationCanceledException = null);

    [LoggerMessage(EventId = 8008, Level = LogLevel.Information, Message = "Key {Key} has been added. Value:\n{RawValue}")]
    public static partial void LogKeyAdd(this ILogger logger, string key, string rawValue);

    [LoggerMessage(EventId = 8009, Level = LogLevel.Information, Message = "Key {Key} has been updated. Old value:\n{OldValue}\nNew value:\n{NewValue}")]
    public static partial void LogKeyUpdate(this ILogger logger, string key, string oldValue, string newValue);

    [LoggerMessage(EventId = 8010, Level = LogLevel.Information, Message = "Key {Key} has been deleted")]
    public static partial void LogKeyDelete(this ILogger logger, string key);

    [LoggerMessage(EventId = 8011, Level = LogLevel.Information, Message = "File {Name} has been inserted")]
    public static partial void LogFileInsert(this ILogger logger, string name);

    [LoggerMessage(EventId = 8012, Level = LogLevel.Information, Message = "File {Name} has been deleted")]
    public static partial void LogFileDelete(this ILogger logger, string name);

    [LoggerMessage(EventId = 8013, Level = LogLevel.Information, Message = "Key {Key} is not found")]
    public static partial void LogKeyIsNotFound(this ILogger logger, string key);

    [LoggerMessage(EventId = 8014, Level = LogLevel.Error, Message = "Cannot sort entries")]
    public static partial void LogSortError(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 8014, Level = LogLevel.Warning, Message = "Cache optimization takes a lot of time: {ElapsedTime}")]
    public static partial void LogSlowSorting(this ILogger logger, TimeSpan elapsedTime);

    [LoggerMessage(EventId = 8015, Level = LogLevel.Error, Message = "Cannot delete expired data for key {Key}")]
    public static partial void LogExpiredKeyDeleteError(this ILogger logger, string key, Exception? exception = null);

    [LoggerMessage(EventId = 8016, Level = LogLevel.Warning, Message = "Request has been cancelled")]
    public static partial void LogCancelledRequest(this ILogger logger, OperationCanceledException operationCanceledException);

    [LoggerMessage(EventId = 8017, Level = LogLevel.Error, Message = "Cannot delete key with {Key}")]
    public static partial void LogFailedKeyDelete(this ILogger logger, string key, Exception exception);
}
