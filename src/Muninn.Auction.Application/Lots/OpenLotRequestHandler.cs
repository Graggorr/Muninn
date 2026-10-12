using FluentResults;
using Muninn.Auction.Application.Contracts;
using Muninn.Auction.Application.Contracts.Commands.Lots;
using Muninn.Auction.Application.Contracts.Common;
using Muninn.Auction.Application.Contracts.Pipeline;

namespace Muninn.Auction.Application.Lots;

internal sealed class OpenLotRequestHandler : IGrainRequestHandler<OpenLotRequest, Lot, ILotGrain>
{
    public Task<Result<Lot>> HandleAsync(OpenLotRequest request, ILotGrain grain, CancellationToken cancellationToken = default)
        => grain.OpenAsync(request, cancellationToken);
}
