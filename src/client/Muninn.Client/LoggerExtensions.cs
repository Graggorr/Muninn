using Microsoft.Extensions.Logging;

namespace Muninn;

internal static partial class LoggerExtensions
{
    [LoggerMessage(EventId = 9000, Level = LogLevel.Error, Message = "Request failed for {FullPath}.\nReason: {Message}")]
    public static partial void LogBadRequest(this ILogger logger, string fullPath, string message);

    [LoggerMessage(EventId = 9001, Level = LogLevel.Error, Message = "Cannot send a request to muninn")]
    public static partial void LogSendAsyncException(this ILogger logger, Exception exception);
}
