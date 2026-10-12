using FluentResults;
using Muninn.Auction.Data.Contracts.Entities;

namespace Muninn.Auction.Data.Contracts.Common;

public interface IProfileRepository<TEntity> : IDeleteRepository<TEntity> where TEntity: BaseEntity
{
    Task<bool> IsUsernameUniqueAsync(string username, CancellationToken cancellationToken = default);
    
    Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default);
    
    Task<Result> UpdatePasswordAsync(Guid id, string hashedPassword, CancellationToken cancellationToken = default);
}
