using FluentResults;
using Muninn.Auction.Application.Contracts.Commands.Lots;
using ApplicationLot = Muninn.Auction.Application.Contracts.Lot;
using ApplicationBid = Muninn.Auction.Application.Contracts.Bid;

namespace Muninn.Auction.Application.Contracts.Common;

[Alias(nameof(ILotGrain))]
public interface ILotGrain : IGrainWithGuidKey
{
    [Alias(nameof(OpenAsync))]
    Task<Result<ApplicationLot>> OpenAsync(OpenLotRequest request, CancellationToken cancellationToken = default);

    [Alias(nameof(GetAsync))]
    Task<Result<ApplicationLot>> GetAsync(CancellationToken cancellationToken = default);

    [Alias(nameof(PlaceBidAsync))]
    Task<Result<ApplicationBid>> PlaceBidAsync(PlaceBidRequest request, CancellationToken cancellationToken = default);

    [Alias(nameof(FinishAsync))]
    Task<Result<ApplicationLot>> FinishAsync(CancellationToken cancellationToken = default);
    
    [Alias(nameof(UpdateLotAsync))]
    Task<Result> UpdateLotAsync(ApplicationLot lot, CancellationToken cancellationToken = default);
}
