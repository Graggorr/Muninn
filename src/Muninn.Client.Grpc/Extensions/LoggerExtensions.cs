using Microsoft.Extensions.Logging;

namespace Muninn.Grpc.Extensions;

internal static partial class LoggerExtensions
{
    [LoggerMessage(Level = LogLevel.Error, EventId = 7001, Message = "Cannot add key {Key} into the Muninn cache.",
        SkipEnabledCheck = true)]
    public static partial void LogAddAsyncError(this ILogger logger, string key, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, EventId = 7002, Message = "Cannot update key {Key} in the Muninn cache.",
        SkipEnabledCheck = true)]
    public static partial void LogUpdateAsyncError(this ILogger logger, string key, Exception exception);
    
    [LoggerMessage(Level = LogLevel.Error, EventId = 7003, Message = "Cannot insert key {Key} into the Muninn cache.",
        SkipEnabledCheck = true)]
    public static partial void LogInsertAsyncError(this ILogger logger, string key, Exception exception);
    
    [LoggerMessage(Level = LogLevel.Error, EventId = 7004, Message = "Cannot remove key {Key} from the Muninn cache.",
        SkipEnabledCheck = true)]
    public static partial void LogRemoveAsyncError(this ILogger logger, string key, Exception exception);
    
    [LoggerMessage(Level = LogLevel.Error, EventId = 7005, Message = "Cannot get key {Key} from the Muninn cache.",
        SkipEnabledCheck = true)]
    public static partial void LogGetAsyncError(this ILogger logger, string key, Exception exception);
    
    [LoggerMessage(Level = LogLevel.Error, EventId = 7006, Message = "Cannot clear the entire Muninn cache.",
        SkipEnabledCheck = true)]
    public static partial void LogClearAsyncError(this ILogger logger, Exception exception);
}
