using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muninn.Auction.Common.Extensions;
using Muninn.Auction.Data.Contracts;
using Muninn.Auction.Data.Contracts.Common;
using Muninn.Auction.Data.Contracts.Entities;
using Muninn.Auction.Data.Extensions;
using static FluentResults.Result;
using static Muninn.Auction.Data.ErrorCode;

namespace Muninn.Auction.Data.Repositories;

internal abstract class BaseProfileRepository<TEntity>(ILogger<IRepository<TEntity>> logger, ApplicationDbContext dbContext)
    : BaseDeleteRepository<TEntity>(logger, dbContext), IProfileRepository<TEntity> where TEntity : BaseProfileEntity
{
    public async Task<bool> IsUsernameUniqueAsync(string username, CancellationToken cancellationToken = default) 
        => !await _query.AnyAsync(x => x.Username.Equals(username), cancellationToken);

    public async Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default)
        => !await _query.AnyAsync(x => x.Email.Equals(email), cancellationToken);

    public async Task<Result> UpdatePasswordAsync(Guid id, string hashedPassword, CancellationToken cancellationToken = default)
    {
        var entity = await _query.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

        if (entity is null)
        {
            return Result.WithRootError(UserNotFound);
        }

        entity.HashedPassword = hashedPassword; 
        await SaveChangesAsync(cancellationToken);
        _logger.UpdatePassword(_name, id);

        return Ok();
    }
}
