namespace Muninn.Auction.Data.Entities;

public sealed record Bid : IEntity
{
    public Guid Id { get; init; }
    
    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public required User Owner { get; init; }
    
    public required Guid OwnerId { get; init; }
    
    public required Lot Lot { get; init; }
    
    public required Guid LotId { get; init; }
}
