using FluentResults;
using Muninn.Auction.Data.Contracts.Common;
using Muninn.Auction.Data.Contracts.Entities;

namespace Muninn.Auction.Data.Contracts;

public interface ILotRepository : IRepository<Lot>
{
    Task<Result> ChangeStartPriceAsync(Guid lotId, ulong price, CancellationToken cancellationToken = default);
    
    Task<Result> ChangePriceAsync(Guid lotId, ulong maxPrice, ulong buyoutPrice, CancellationToken cancellationToken = default);
    
    Task<Result> FinishAsync(Guid lotId, CancellationToken cancellationToken = default);
    
    Task<Result<Bid>> PlaceBidAsync(Guid lotId, Bid bid, CancellationToken cancellationToken = default);
}
