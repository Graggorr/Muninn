using Microsoft.Extensions.Logging;
using static Microsoft.Extensions.Logging.LogLevel;

namespace Muninn.Auction.Application.Extensions;

internal static partial class LoggerExtensions
{
    [LoggerMessage(Level = Debug, Message = "{Grain} {Id} rehydrated from the database")]
    public static partial void GrainRehydrated(this ILogger logger, string grain, Guid id);

    [LoggerMessage(Level = Debug, Message = "{Grain} {Id} has no database record, staying empty")]
    public static partial void GrainRehydrationEmpty(this ILogger logger, string grain, Guid id);

    [LoggerMessage(Level = Information, Message = "Bid {BidId} of {Amount} accepted on lot {LotId} by {UserId}")]
    public static partial void BidAccepted(this ILogger logger, Guid bidId, ulong amount, Guid lotId, Guid userId);

    [LoggerMessage(Level = Information, Message = "Lot {LotId} finished")]
    public static partial void LotFinished(this ILogger logger, Guid lotId);
    
    [LoggerMessage(Level = Information, Message = "Lot {LotId} has been updated")]
    public static partial void LotUpdated(this ILogger logger, Guid lotId);

    [LoggerMessage(Level = Debug, Message = "{Request} has been finished in {Time} {Status}")]
    public static partial void RequestFinished(this ILogger logger, string request, TimeSpan time, string status);

    [LoggerMessage(Level = Error, Message = "{Request} has failed due to error. Request body:\n{RequestBody}")]
    public static partial void RequestError(this ILogger logger, string request, string requestBody, Exception exception);
}
