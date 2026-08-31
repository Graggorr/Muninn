using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muninn.Auction.Common.Extensions;
using Muninn.Auction.Data.Entities;
using static FluentResults.Result;
using static Muninn.Auction.Data.Enums.ErrorCode;

namespace Muninn.Auction.Data.Repositories.Internal;

internal abstract class BaseProfileRepository<TEntity>(ILogger<IRepository<TEntity>> logger, ApplicationDbContext dbContext)
    : BaseDeleteRepository<TEntity>(logger, dbContext), IProfileRepository<TEntity> where TEntity : BaseProfileEntity
{
    public async Task<Result> IsUsernameUniqueAsync(string username, CancellationToken cancellationToken = default)
    {
        var result = await _query.AnyAsync(x => x.Username.Equals(username), cancellationToken);
        
        return result ? ResultBase.WithRootError(UsernameNotUnique) : Ok();
    }

    public async Task<Result> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default)
    {
        var result = await _query.AnyAsync(x => x.Email.Equals(email), cancellationToken);

        return result ? Result.WithRootError(EmailNotUnique) : Ok();
    }

    public async Task<Result> UpdatePasswordAsync(Guid id, string hashedPassword, CancellationToken cancellationToken = default)
    {
        var entity = await _query.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

        if (entity is null)
        {
            return Result.WithRootError(UserNotFound);
        }

        entity.HashedPassword = hashedPassword; 
        await SaveChangesAsync(cancellationToken);

        return Ok();
    }
}
