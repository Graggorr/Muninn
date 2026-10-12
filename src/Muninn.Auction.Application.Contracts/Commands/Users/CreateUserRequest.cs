using Muninn.Auction.Application.Contracts.Common;
using Muninn.Auction.Application.Contracts.Pipeline;

namespace Muninn.Auction.Application.Contracts.Commands.Users;

public sealed record CreateUserRequest : IGrainRequest<User, IUserGrain>
{
    public required Guid GrainId { get; init; }

    public required string Username { get; init; }

    public required string Password { get; init; }

    public required string Email { get; init; }

    public required string MobileNumber { get; init; }
}
