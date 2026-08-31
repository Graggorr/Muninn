using Microsoft.Extensions.Logging;
using static Microsoft.Extensions.Logging.LogLevel;

namespace Muninn.Auction.Data.Extensions;

internal static partial class LoggerExtensions
{
    [LoggerMessage(Level = Error, Message = "The operation has been cancelled", SkipEnabledCheck = true)]
    public static partial void OperationCancelled(this ILogger logger, OperationCanceledException exception);

    [LoggerMessage(Level = Debug, Message = "New {Name} has been added with ID {id}", SkipEnabledCheck = true)]
    public static partial void AddEntity(this ILogger logger, string name, Guid id);
    
    [LoggerMessage(Level = Error, Message = "{Name} cannot be added due to error, ID: {Id}", SkipEnabledCheck = true)]
    public static partial void AddEntityError(this ILogger logger, string name, Guid id, Exception exception);
    
    [LoggerMessage(Level = Debug, Message = "{Name} has been deleted with ID: {Id}", SkipEnabledCheck = true)]
    public static partial void DeleteEntity(this ILogger logger, string name, Guid id);

    [LoggerMessage(Level = Debug, Message = "{Name} has not been found to delete, skiping. ID: {Id}", SkipEnabledCheck = true)]
    public static partial void DeleteEntityNotFound(this ILogger logger, string name, Guid id);
    
    [LoggerMessage(Level = Debug, Message = "Password has been updated for {Name} with ID {id}", SkipEnabledCheck = true)]
    public static partial void UpdatePassword(this ILogger logger, string name, Guid id);
}
