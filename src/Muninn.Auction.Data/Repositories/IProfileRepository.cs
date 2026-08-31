using FluentResults;
using Muninn.Auction.Data.Entities;

namespace Muninn.Auction.Data.Repositories;

public interface IProfileRepository<TEntity> : IRepository<TEntity>, IDeleteRepository<TEntity> where TEntity: BaseEntity
{
    Task<Result> IsUsernameUniqueAsync(string username, CancellationToken cancellationToken = default);
    
    Task<Result> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default);
    
    Task<Result> UpdatePasswordAsync(Guid id, string hashedPassword, CancellationToken cancellationToken = default);
}
