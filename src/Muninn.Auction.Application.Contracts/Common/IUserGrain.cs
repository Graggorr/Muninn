using FluentResults;
using ApplicationUser = Muninn.Auction.Application.Contracts.User;

namespace Muninn.Auction.Application.Contracts.Common;

public interface IUserGrain : IGrainWithGuidKey
{
    Task<Result> CreateUserAsync(ApplicationUser applicationUser, string hashedPassword, CancellationToken cancellationToken = default);
}
