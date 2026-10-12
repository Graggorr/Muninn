namespace Muninn.Auction.Data.Contracts.Entities;

public sealed record User : BaseProfileEntity
{
    public required string MobileNumber { get; set; }
    
    public bool IsMobileNumberConfirmed { get; set; }
    
    public bool IsEmailConfirmed { get; set; }
    
    public ulong Amount { get; set; }
    
    public List<Lot> HistoryLots { get; init; } = [];
}
