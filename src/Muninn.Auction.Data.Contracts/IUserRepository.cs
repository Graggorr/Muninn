using FluentResults;
using Muninn.Auction.Data.Contracts.Common;
using Muninn.Auction.Data.Contracts.Entities;

namespace Muninn.Auction.Data.Contracts;

public interface IUserRepository : IProfileRepository<User>
{
    Task<Result> UpdateAmountAsync(Guid userId, ulong amount, CancellationToken cancellationToken = default);
    
    Task<Result> ConfirmEmailAsync(Guid userId, CancellationToken cancellationToken = default);
    
    Task<Result> ConfirmMobileNumberAsync(Guid userId, CancellationToken cancellationToken = default);
    
    Task<Result> AddLotAsync(Guid userId, Lot lot, CancellationToken cancellationToken = default);
    
    Task<bool> IsMobileNumberUniqueAsync(string mobileNumber, CancellationToken cancellationToken = default);
    
    Task<Result<List<Bid>>> GetUserBidsAsync(Guid userId, CancellationToken cancellationToken = default);
    
    Task<Result<List<Lot>>> GetUserAllLotsAsync(Guid userId, CancellationToken cancellationToken = default);
    
    Task<Result<List<Lot>>> GetUserWonLotsAsync(Guid userId, CancellationToken cancellationToken = default);
}
