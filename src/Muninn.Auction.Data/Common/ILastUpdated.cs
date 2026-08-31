namespace Muninn.Auction.Data.Common;

public interface ILastUpdated 
{ 
    DateTime LastUpdatedAt { get; set; }
    
    string LastUpdatedBy { get; set; }
}
