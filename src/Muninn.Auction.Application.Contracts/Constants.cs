using static Muninn.Auction.Common.Constants;

namespace Muninn.Auction.Application.Contracts;

public static class Constants
{
    public const string GrainStorageName = "auction-store";

    public const string LotStateName = "lot";

    public const string UserStateName = "user";
    
    public const string RedisSectionKey = "Redis";

    public const string OrleansSectionKey = "Orleans";

    public static readonly string[] PropagatedContextKeys =
        [UserNameTagKey, CallerNameTagKey, CallerIdTagKey];
}
