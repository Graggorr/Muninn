using FluentResults;
using Muninn.Auction.Data.Entities;

namespace Muninn.Auction.Data.Repositories;

public interface ILotRepository : IRepository<Lot>
{
    Task<Result> ChangeStartPriceAsync(Guid lotId, ulong price, CancellationToken cancellationToken = default);
    
    Task<Result> ChangePriceAsync(Guid lotId, ulong maxPrice, ulong buyoutPrice, CancellationToken cancellationToken = default);
    
    Task<Result> UpdateLeaderAsync(Guid lotId, Guid leaderId, CancellationToken cancellationToken = default);
    
    Task<Result> FinishAsync(Guid lotId, CancellationToken cancellationToken = default);
    
    Task<Result> PlaceBidAsync(Guid lotId, Bid bid, CancellationToken cancellationToken = default);
}
