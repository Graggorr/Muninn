namespace Muninn.Auction.Data.Common;

public interface ICreated
{
    DateTime CreatedAt { get; set; }
    
    string CreatedBy { get; set; }
}
