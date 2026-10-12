namespace Muninn.Auction.Data.Contracts.Common;

public interface ICreated
{
    DateTime CreatedAt { get; set; }
    
    string CreatedBy { get; set; }
}
