using FluentResults;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Muninn.Auction.Application.Contracts.Commands.Lots;
using Muninn.Auction.Application.Contracts.Common;
using Muninn.Auction.Application.Contracts.States;
using Muninn.Auction.Application.Extensions;
using Muninn.Auction.Data.Contracts;
using static FluentResults.Result;
using static Muninn.Auction.Data.ErrorCode;
using static Muninn.Auction.Application.Contracts.Constants;
using ApplicationBid = Muninn.Auction.Application.Contracts.Bid;
using ApplicationLot = Muninn.Auction.Application.Contracts.Lot;
using DataBid = Muninn.Auction.Data.Contracts.Entities.Bid;
using DataLot = Muninn.Auction.Data.Contracts.Entities.Lot;

namespace Muninn.Auction.Application.Grains;

internal sealed class LotGrain(
    [PersistentState(LotStateName, GrainStorageName)] IPersistentState<LotState> persistentState,
    IServiceScopeFactory scopeFactory,
    ILogger<LotGrain> logger) : Grain, ILotGrain
{
    private readonly IPersistentState<LotState> _persistentState = persistentState;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger _logger = logger;

    private LotState State => _persistentState.State;

    private Guid LotId => this.GetPrimaryKey();

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        if (!State.Exists)
        {
            await RehydrateAsync(cancellationToken);
        } 
        
        await base.OnActivateAsync(cancellationToken);
    }

    public async Task<Result<ApplicationLot>> OpenAsync(OpenLotRequest request, CancellationToken cancellationToken = default)
    {
        if (State.Exists)
        {
            return Fail<ApplicationLot>(nameof(LotAlreadyExists));
        }

        var lot = request.Adapt<DataLot>();

        await using (var scope = _scopeFactory.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<ILotRepository>().AddAsync(lot, cancellationToken);
        }

        request.Adapt(State);

        await _persistentState.WriteStateAsync(cancellationToken);

        return Ok(ToSnapshot());
    }

    public Task<Result<ApplicationLot>> GetAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(State.Exists
            ? Ok(ToSnapshot())
            : Fail<ApplicationLot>(nameof(LotNotFound)));

    public async Task<Result<ApplicationBid>> PlaceBidAsync(PlaceBidRequest request, CancellationToken cancellationToken = default)
    {
        if (!State.Exists)
        {
            return Fail(nameof(LotNotFound));
        }

        if (State.IsFinished)
        {
            return Fail(nameof(LotAlreadyFinished));
        }

        if (request.UserId == State.OwnerId)
        {
            return Fail(nameof(OwnerCannotBidOnOwnLot));
        }

        if (request.Amount <= State.CurrentPrice)
        {
            return Fail(nameof(BidTooLow));
        }

        var isBuyout = State.BuyoutPrice > 0 && request.Amount >= State.BuyoutPrice;
        var bid = request.Adapt<DataBid>() with { LotId = LotId };
        
        await using (var scope = _scopeFactory.CreateAsyncScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<ILotRepository>();
            var placed = await repository.PlaceBidAsync(LotId, bid, cancellationToken);

            if (placed.IsFailed)
            {
                return Fail(placed.Errors);
            }
            
            if (isBuyout)
            {
                var finished = await repository.FinishAsync(LotId, cancellationToken);

                if (finished.IsFailed)
                {
                    return Fail(finished.Errors);
                }
            }
        }

        State.CurrentPrice = bid.Amount;
        State.LeaderId = bid.OwnerId;
        State.BidCount++;

        if (isBuyout)
        {
            State.IsFinished = true;
            State.WinnerBidId = bid.Id;
        }

        await _persistentState.WriteStateAsync(cancellationToken);
        _logger.BidAccepted(bid.Id, bid.Amount, LotId, bid.OwnerId);

        return Ok(bid.Adapt<ApplicationBid>() with { IsBuyout = isBuyout });
    }

    public async Task<Result<ApplicationLot>> FinishAsync(CancellationToken cancellationToken = default)
    {
        if (!State.Exists)
        {
            return Fail(nameof(LotNotFound));
        }

        if (State.IsFinished)
        {
            return Fail(nameof(LotAlreadyFinished));
        }

        await using (var scope = _scopeFactory.CreateAsyncScope())
        {
            var finished = await scope.ServiceProvider.GetRequiredService<ILotRepository>().FinishAsync(LotId, cancellationToken);

            if (finished.IsFailed)
            {
                return Fail(finished.Errors);
            }
        }

        State.IsFinished = true;
        await _persistentState.WriteStateAsync(cancellationToken);
        _logger.LotFinished(LotId);

        return Ok(ToSnapshot());
    }
    
    public async Task<Result> UpdateLotAsync(ApplicationLot applicationLot, CancellationToken cancellationToken = default)
    {
        if (!State.Exists)
        {
            return Fail(nameof(LotNotFound));
        }
        
        var lot = applicationLot.Adapt<DataLot>();
        
        await using (var scope = _scopeFactory.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<ILotRepository>().UpdateAsync(lot, cancellationToken);
        }
        
        lot.Adapt(State);
        await _persistentState.WriteStateAsync(cancellationToken);
        _logger.LotUpdated(LotId);
        
        return Ok();
    }
    
    private async Task RehydrateAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var lot = await scope.ServiceProvider.GetRequiredService<ILotRepository>().GetByIdAsync(LotId, cancellationToken);

        if (lot is null)
        {
            _logger.GrainRehydrationEmpty(nameof(LotGrain), LotId);

            return;
        }

        lot.Adapt(State);

        await _persistentState.WriteStateAsync(cancellationToken);
        _logger.GrainRehydrated(nameof(LotGrain), LotId);
    }

    private ApplicationLot ToSnapshot() => State.Adapt<ApplicationLot>() with { Id = LotId };
}
