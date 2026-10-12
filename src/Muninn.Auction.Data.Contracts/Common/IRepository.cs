using Muninn.Auction.Data.Contracts.Entities;

namespace Muninn.Auction.Data.Contracts.Common;

public interface IRepository<TEntity> where TEntity : class, IEntity
{
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
}
