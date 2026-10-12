using FluentResults;
using Mapster;
using Muninn.Auction.Application.Common;
using Muninn.Auction.Application.Contracts;
using Muninn.Auction.Application.Contracts.Commands.Users;
using Muninn.Auction.Application.Contracts.Common;
using Muninn.Auction.Application.Contracts.Pipeline;

namespace Muninn.Auction.Application.Users;

internal sealed class CreateUserRequestHandler(IPasswordHasher passwordHasher)
    : IGrainRequestHandler<CreateUserRequest, User, IUserGrain>
{
    private readonly IPasswordHasher _passwordHasher = passwordHasher;

    public async Task<Result<User>> HandleAsync(CreateUserRequest request, IUserGrain grain, CancellationToken cancellationToken = default)
    {
        var hashedPassword = _passwordHasher.HashPassword(request.Password);
        var user = request.Adapt<User>() with { Id = request.GrainId };
        var result = await grain.CreateUserAsync(user, hashedPassword, cancellationToken);

        return result.IsFailed ? result : user;
    }
}
