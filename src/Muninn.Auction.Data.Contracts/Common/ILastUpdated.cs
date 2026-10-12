namespace Muninn.Auction.Data.Contracts.Common;

public interface ILastUpdated 
{ 
    DateTime LastUpdatedAt { get; set; }
    
    string LastUpdatedBy { get; set; }
}
