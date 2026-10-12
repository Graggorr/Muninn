namespace Muninn.Auction.Application.Contracts;

[GenerateSerializer]
public sealed record Bid
{
    [Id(0)]
    public required Guid Id { get; init; }

    [Id(1)]
    public required Guid LotId { get; init; }

    [Id(2)]
    public required ulong Amount { get; init; }

    [Id(3)]
    public required bool IsBuyout { get; init; }
}
