using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muninn.Auction.Data.Common;
using Muninn.Auction.Data.Entities;
using static FluentResults.Result;

namespace Muninn.Auction.Data.Repositories.Internal;

internal abstract class BaseDeleteRepository<TEntity>(ILogger<IRepository<TEntity>> logger, ApplicationDbContext dbContext)
    : BaseRepository<TEntity>(logger, dbContext), IDeleteRepository<TEntity> where TEntity: class, IEntity, IDeleted
{
    public override async Task<TEntity?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _query.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

        return entity is IDeleted { IsDeleted: true } ? null : entity;
    }

    public async Task<Result<TEntity?>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _query.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

        if (entity is null)
        {
            return Ok();
        }

        if (!entity.IsDeleted)
        {
            entity.IsDeleted = true;
            await SaveChangesAsync(cancellationToken);
        }

        return entity;
    }
}
