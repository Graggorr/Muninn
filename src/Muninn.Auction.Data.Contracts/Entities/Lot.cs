namespace Muninn.Auction.Data.Contracts.Entities;

public sealed record Lot : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public ulong StartPrice { get; set; }
    
    public ulong MaxPrice { get; set; }
    
    public ulong BuyoutPrice { get; set; }

    public DateTime FinishTimeUtc { get; init; }
    
    public bool IsFinished { get; set; }
    
    public required Guid OwnerId { get; init; }
    
    public User Owner { get; init; } = null!;

    public Bid? WinnerBid { get; set; }
    
    public Guid? WinnerBidId { get; set; }
    
    public List<Bid> Bids { get; init; } = [];
    
    public List<User> Participants { get; init; } = [];
}
