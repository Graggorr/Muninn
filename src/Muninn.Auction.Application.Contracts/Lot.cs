namespace Muninn.Auction.Application.Contracts;

[GenerateSerializer]
public sealed record Lot
{
    [Id(0)]
    public required Guid Id { get; init; }

    [Id(1)]
    public required Guid OwnerId { get; init; }

    [Id(2)]
    public required string Title { get; init; }

    [Id(3)]
    public required string Description { get; init; }

    [Id(4)]
    public required ulong StartPrice { get; init; }

    [Id(5)]
    public required ulong CurrentPrice { get; init; }

    [Id(6)]
    public required ulong BuyoutPrice { get; init; }

    [Id(7)]
    public Guid? LeaderId { get; init; }

    [Id(8)]
    public Guid? WinnerBidId { get; init; }

    [Id(9)]
    public required bool IsFinished { get; init; }

    [Id(10)]
    public required int BidCount { get; init; }

    [Id(11)]
    public required List<Bid> Bids { get; init; }
}
