using FluentResults;
using Muninn.Auction.Data.Contracts.Entities;

namespace Muninn.Auction.Data.Contracts.Common;

public interface IDeleteRepository<TEntity> : IRepository<TEntity> where TEntity : class, IEntity, IDeleted
{
    Task<Result<TEntity?>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
