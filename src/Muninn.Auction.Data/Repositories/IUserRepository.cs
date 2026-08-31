using FluentResults;
using Muninn.Auction.Data.Entities;

namespace Muninn.Auction.Data.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<Result> UpdateAmountAsync(Guid userId, ulong amount, CancellationToken cancellationToken = default);
    
    Task<Result> ConfirmEmailAsync(Guid userId, CancellationToken cancellationToken = default);
    
    Task<Result> ConfirmMobileNumberAsync(Guid userId, CancellationToken cancellationToken = default);
    
    Task<Result> AddLotAsync(Guid userId, Lot lot, CancellationToken cancellationToken = default);
    
    Task<List<Bid>> GetUserBidsAsync(Guid userId, CancellationToken cancellationToken = default);
    
    Task<List<Lot>> GetUserAllLotsAsync(Guid userId, CancellationToken cancellationToken = default);
    
    Task<List<Lot>> GetUserWonLotsAsync(Guid userId, CancellationToken cancellationToken = default);
    
    Task<Result> IsMobileNumberUniqueAsync(string mobileNumber, CancellationToken cancellationToken = default);
}
