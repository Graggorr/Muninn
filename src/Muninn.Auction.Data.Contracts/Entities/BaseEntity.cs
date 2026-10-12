using Muninn.Auction.Data.Contracts.Common;

namespace Muninn.Auction.Data.Contracts.Entities;

public abstract record BaseEntity : IEntity, ILastUpdated, IDeleted
{
    public Guid Id { get; init; }

    public DateTime LastUpdatedAt { get; set; }

    public string LastUpdatedBy { get; set; } = string.Empty;
    
    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }
    
    public string CreatedBy { get; set; } = string.Empty;
}
