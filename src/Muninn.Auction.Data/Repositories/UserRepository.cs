using Ardalis.Specification.EntityFrameworkCore;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muninn.Auction.Common.Extensions;
using Muninn.Auction.Data.Contracts;
using Muninn.Auction.Data.Contracts.Common;
using Muninn.Auction.Data.Contracts.Entities;
using Muninn.Auction.Data.Specifications;
using static FluentResults.Result;
using static Muninn.Auction.Data.ErrorCode;

namespace Muninn.Auction.Data.Repositories;

internal sealed class UserRepository(ILogger<IRepository<User>> logger, ApplicationDbContext dbContext) 
    : BaseProfileRepository<User>(logger, dbContext), IUserRepository
{
    public async Task<Result> UpdateAmountAsync(Guid userId, ulong amount, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdCoreAsync(userId, cancellationToken);

        if (user is null)
        {
            return Result.WithRootError(UserNotFound);
        }
        
        user.Amount += amount;
        await SaveChangesAsync(cancellationToken);
        
        return Ok();
    }

    public async Task<Result> ConfirmEmailAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdCoreAsync(userId, cancellationToken);

        if (user is null)
        {
            return Result.WithRootError(UserNotFound);
        }
        
        user.IsEmailConfirmed = true;
        await SaveChangesAsync(cancellationToken);
        
        return Ok();
    }

    public async Task<Result> ConfirmMobileNumberAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdCoreAsync(userId, cancellationToken);

        if (user is null)
        {
            return Result.WithRootError(UserNotFound);
        }
        
        user.IsMobileNumberConfirmed = true;
        await SaveChangesAsync(cancellationToken);
        
        return Ok();
    }

    public async Task<Result> AddLotAsync(Guid userId, Lot lot, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdCoreAsync(userId, cancellationToken);

        if (user is null)
        {
            return Result.WithRootError(UserNotFound);
        }
        
        user.HistoryLots.Add(lot);
        await SaveChangesAsync(cancellationToken);
        
        return Ok();
    }

    public async Task<bool> IsMobileNumberUniqueAsync(string mobileNumber, CancellationToken cancellationToken = default)
        => !await _query.AnyAsync(user => user.MobileNumber.Equals(mobileNumber), cancellationToken);

    public async Task<Result<List<Bid>>> GetUserBidsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await GetByIdCoreAsync(userId, cancellationToken);

        if (user is null)
        {
            return Result.WithRootError(UserNotFound);
        }
        
        return user.HistoryLots.SelectMany(lot => lot.Bids).ToList();
    }

    public async Task<Result<List<Lot>>> GetUserAllLotsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return Result.WithRootError(UserNotFound);
        }
        
        return user.HistoryLots;
    }

    public async Task<Result<List<Lot>>> GetUserWonLotsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return Result.WithRootError(UserNotFound);
        }
        
        return user.HistoryLots.Where(lot => lot.WinnerBid is { } bid && bid.OwnerId == userId).ToList();
    }

    public override async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var specification = new UserSpecification(id);
        var user = await _query.WithSpecification(specification).SingleOrDefaultAsync(cancellationToken);

        if (user is { IsDeleted: true })
        {
            user = null;
        }

        return user;
    }
}
