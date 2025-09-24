namespace Muninn.Api.Extensions;

public static partial class LoggerExtensions
{
    [LoggerMessage(EventId = 1000, Level = LogLevel.Error, Message = "Invalid API-Key has been used from {IpAddress}")]
    public static partial void LogInvalidApiKey(this ILogger logger, string ipAddress);
}
