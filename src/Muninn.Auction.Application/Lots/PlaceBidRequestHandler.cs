using FluentResults;
using Muninn.Auction.Application.Contracts;
using Muninn.Auction.Application.Contracts.Commands.Lots;
using Muninn.Auction.Application.Contracts.Common;
using Muninn.Auction.Application.Contracts.Pipeline;

namespace Muninn.Auction.Application.Lots;

internal sealed class PlaceBidRequestHandler : IGrainRequestHandler<PlaceBidRequest, Bid, ILotGrain>
{
    public Task<Result<Bid>> HandleAsync(PlaceBidRequest request, ILotGrain grain, CancellationToken cancellationToken = default)
        => grain.PlaceBidAsync(request, cancellationToken);
}
