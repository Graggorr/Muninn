using FluentResults;
using Muninn.Auction.Data.Common;
using Muninn.Auction.Data.Entities;

namespace Muninn.Auction.Data.Repositories;

public interface IDeleteRepository<TEntity> : IRepository<TEntity> where TEntity : class, IEntity, IDeleted
{
    Task<Result<TEntity?>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
