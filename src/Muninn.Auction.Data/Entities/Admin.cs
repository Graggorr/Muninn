using Muninn.Auction.Data.Enums;

namespace Muninn.Auction.Data.Entities;

public sealed record Admin : BaseProfileEntity
{
    public Permission Permission { get; set; }
}
