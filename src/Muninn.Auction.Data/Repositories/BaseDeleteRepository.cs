using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muninn.Auction.Data.Contracts;
using Muninn.Auction.Data.Contracts.Common;
using Muninn.Auction.Data.Contracts.Entities;
using Muninn.Auction.Data.Extensions;
using static FluentResults.Result;

namespace Muninn.Auction.Data.Repositories;

internal abstract class BaseDeleteRepository<TEntity>(ILogger<IRepository<TEntity>> logger, ApplicationDbContext dbContext)
    : BaseRepository<TEntity>(logger, dbContext), IDeleteRepository<TEntity> where TEntity: class, IEntity, IDeleted
{
    public override Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) 
        => GetByIdCoreAsync(id, cancellationToken);

    public async Task<Result<TEntity?>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _query.SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);

        if (entity is null)
        {
            _logger.DeleteEntityNotFound(_name, id);
            
            return Ok();
        }

        if (!entity.IsDeleted)
        {
            entity.IsDeleted = true;
            await SaveChangesAsync(cancellationToken);
            _logger.DeleteEntity(_name, id);
        }

        return entity;
    }

    protected async Task<TEntity?> GetByIdCoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _query.SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);

        return entity is IDeleted { IsDeleted: true } ? null : entity;
    }
}
