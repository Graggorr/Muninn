namespace Muninn.Api.Extensions;

public static partial class LoggerExtensions
{
    [LoggerMessage(EventId = 1000, Level = LogLevel.Error, Message = "Invalid API-Key has been used: {ApiKey}")]
    public static partial void LogInvalidApiKey(this ILogger logger, string apiKey);
}
