using Muninn.Auction.Data.Entities;

namespace Muninn.Auction.Data.Repositories;

public interface IRepository<TEntity> where TEntity : class, IEntity
{
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    
    Task<TEntity?> GetById(Guid id, CancellationToken cancellationToken = default);
    
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
}
