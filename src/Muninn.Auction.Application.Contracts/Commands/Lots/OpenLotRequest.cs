using Muninn.Auction.Application.Contracts.Common;
using Muninn.Auction.Application.Contracts.Pipeline;

namespace Muninn.Auction.Application.Contracts.Commands.Lots;

[GenerateSerializer]
public sealed record OpenLotRequest : IGrainRequest<Lot, ILotGrain>
{
    [Id(0)]
    public required Guid LotId { get; init; }

    [Id(1)]
    public required Guid OwnerId { get; init; }

    [Id(2)]
    public required string Title { get; init; }

    [Id(3)]
    public required string Description { get; init; }

    [Id(4)]
    public required ulong StartPrice { get; init; }

    [Id(5)]
    public required ulong BuyoutPrice { get; init; }

    [Id(6)]
    public required ulong MaxPrice { get; init; }

    public Guid GrainId => LotId;
}
