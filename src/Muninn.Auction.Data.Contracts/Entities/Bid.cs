namespace Muninn.Auction.Data.Contracts.Entities;

public sealed record Bid : IEntity
{
    public Guid Id { get; init; }
    
    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public required ulong Amount { get; init; }

    public required Guid OwnerId { get; init; }

    public User Owner { get; init; } = null!;

    public required Guid LotId { get; init; }

    public Lot Lot { get; init; } = null!;
}
