using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muninn.Auction.Data.Contracts;
using Muninn.Auction.Data.Contracts.Common;
using Muninn.Auction.Data.Contracts.Entities;
using Muninn.Auction.Data.Extensions;

namespace Muninn.Auction.Data.Repositories;

internal abstract class BaseRepository<TEntity>(ILogger<IRepository<TEntity>> logger, ApplicationDbContext dbContext)
    : IRepository<TEntity> where TEntity : class, IEntity
{
    protected readonly ApplicationDbContext _dbContext = dbContext;
    protected readonly ILogger _logger = logger;
    protected readonly DbSet<TEntity> _query = dbContext.Set<TEntity>();
    protected readonly string _name = typeof(TEntity).Name;

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _query.Add(entity);
        await SaveChangesAsync(cancellationToken);
        _logger.AddEntity(_name, entity.Id);
    }

    public abstract Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _query.Update(entity);
        await SaveChangesAsync(cancellationToken);
    }
    
    protected Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) 
        => _dbContext.SaveChangesAsync(cancellationToken);
}
