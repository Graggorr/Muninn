namespace Muninn.Auction.Data.Entities;

public record BaseProfileEntity : BaseEntity
{
    public required string Username { get; set; }
    
    public required string HashedPassword { get; set; }
    
    public required string Email { get; set; }
}
