using Muninn.Auction.Application.Contracts.Common;
using Muninn.Auction.Application.Contracts.Pipeline;

namespace Muninn.Auction.Application.Contracts.Commands.Lots;

[GenerateSerializer]
public sealed record PlaceBidRequest : IGrainRequest<Bid, ILotGrain>
{
    [Id(0)]
    public required Guid LotId { get; init; }

    [Id(1)]
    public required Guid UserId { get; init; }

    [Id(2)]
    public required ulong Amount { get; init; }

    public Guid GrainId => LotId;
}
