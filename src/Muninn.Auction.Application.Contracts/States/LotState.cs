namespace Muninn.Auction.Application.Contracts.States;

[GenerateSerializer]
public sealed record LotState
{
    [Id(0)] 
    public bool Exists { get; set; }

    [Id(1)] 
    public Guid OwnerId { get; set; }

    [Id(2)] 
    public string Title { get; set; } = string.Empty;

    [Id(3)] 
    public string Description { get; set; } = string.Empty;

    [Id(4)]
    public ulong StartPrice { get; set; }
    
    [Id(5)] 
    public ulong CurrentPrice { get; set; }

    [Id(6)] 
    public ulong BuyoutPrice { get; set; }

    [Id(7)] 
    public Guid? LeaderId { get; set; }

    [Id(8)] 
    public Guid? WinnerBidId { get; set; }

    [Id(9)] 
    public bool IsFinished { get; set; }

    [Id(10)] 
    public int BidCount { get; set; }

    [Id(11)]
    public required List<Bid> Bids { get; set; }
}
