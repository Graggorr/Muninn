using FluentResults;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Muninn.Auction.Application.Contracts.Common;
using Muninn.Auction.Application.Contracts.States;
using Muninn.Auction.Data.Contracts;
using static FluentResults.Result;
using static Muninn.Auction.Application.Contracts.Constants;
using ApplicationUser = Muninn.Auction.Application.Contracts.User;
using DataUser = Muninn.Auction.Data.Contracts.Entities.User;

namespace Muninn.Auction.Application.Grains;

internal sealed class UserGrain([PersistentState(UserStateName, GrainStorageName)] IPersistentState<UserState> persistentState,
    IServiceScopeFactory scopeFactory,
    ILogger<UserGrain> logger) : Grain, IUserGrain
{
    private readonly IPersistentState<UserState> _persistentState = persistentState;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger _logger = logger;
    
    private UserState State => _persistentState.State;
    private Guid UserId => this.GetPrimaryKey();
    
    public async Task<Result> CreateUserAsync(ApplicationUser applicationUser, string hashedPassword,
        CancellationToken cancellationToken = default)
    {
        var user = applicationUser.Adapt<DataUser>() with { Id = UserId, HashedPassword = hashedPassword };
        await using var scope = _scopeFactory.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<IUserRepository>().AddAsync(user, cancellationToken);
        user.Adapt(State);
        await _persistentState.WriteStateAsync(cancellationToken);
        
        return Ok();
    }
}
