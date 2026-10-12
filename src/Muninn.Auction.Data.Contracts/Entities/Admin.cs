using Muninn.Auction.Data.Contracts.Enums;

namespace Muninn.Auction.Data.Contracts.Entities;

public sealed record Admin : BaseProfileEntity
{
    public Permission Permission { get; set; }
}
