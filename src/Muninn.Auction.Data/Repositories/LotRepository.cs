using Ardalis.Specification.EntityFrameworkCore;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Muninn.Auction.Common.Extensions;
using Muninn.Auction.Data.Contracts;
using Muninn.Auction.Data.Contracts.Common;
using Muninn.Auction.Data.Contracts.Entities;
using Muninn.Auction.Data.Specifications;
using static FluentResults.Result;
using static Muninn.Auction.Data.ErrorCode;

namespace Muninn.Auction.Data.Repositories;

internal sealed class LotRepository(ILogger<IRepository<Lot>> logger, ApplicationDbContext dbContext)
    : BaseDeleteRepository<Lot>(logger, dbContext), ILotRepository
{
    public async Task<Result> ChangeStartPriceAsync(Guid lotId, ulong price, CancellationToken cancellationToken = default)
    {
        var lotResult = await GetValidLotByIdAsync(lotId, cancellationToken);

        if (lotResult.IsFailed)
        {
            return lotResult.ToResult();
        }

        var lot = lotResult.Value;

        if (lot.Bids.Any())
        {
            return Result.WithRootError(StartPriceCannotBeChanged);
        }

        lot.StartPrice = price;
        await SaveChangesAsync(cancellationToken);

        return Ok();
    }

    public async Task<Result> ChangePriceAsync(Guid lotId, ulong maxPrice, ulong buyoutPrice, CancellationToken cancellationToken = default)
    {
        var lotResult = await GetValidLotByIdAsync(lotId, cancellationToken);

        if (lotResult.IsFailed)
        {
            return lotResult.ToResult();
        }

        var lot = lotResult.Value;
        var result = new Result();

        if (lot.MaxPrice > maxPrice)
        {
            result.WithError(MaxPriceLower);
        }

        if (lot.BuyoutPrice > buyoutPrice)
        {
            result.WithError(BuyoutPriceLower);
        }

        if (lot.StartPrice > maxPrice)
        {
            result.WithError(StartPriceHigherThanMaxPrice);
        }

        if (lot.StartPrice > buyoutPrice)
        {
            result.WithError(StartPriceHigherThanBuyoutPrice);
        }

        if (result.IsSuccess)
        {
            lot.MaxPrice = maxPrice;
            lot.BuyoutPrice = buyoutPrice;
            await SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    public async Task<Result> FinishAsync(Guid lotId, CancellationToken cancellationToken = default)
    {
        var lotResult = await GetValidLotByIdAsync(lotId, cancellationToken);

        if (lotResult.IsFailed)
        {
            return lotResult.ToResult();
        }

        lotResult.Value.IsFinished = true;
        await SaveChangesAsync(cancellationToken);
        
        return Ok();
    }

    public async Task<Result<Bid>> PlaceBidAsync(Guid lotId, Bid bid, CancellationToken cancellationToken = default)
    {
        var lotResult = await GetValidLotByIdAsync(lotId, cancellationToken);

        if (lotResult.IsFailed)
        {
            return lotResult.ToResult();
        }
        
        var lot = lotResult.Value;
        _dbContext.Bids.Add(bid);
        lot.Bids.Add(bid);
        await SaveChangesAsync(cancellationToken);
        
        return Ok();
    }

    public override Task<Lot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _query.WithSpecification(new LotSpecification(id)).SingleOrDefaultAsync(cancellationToken);

    private async Task<Result<Lot>> GetValidLotByIdAsync(Guid lotId, CancellationToken cancellationToken = default)
    {
        var lot = await GetByIdAsync(lotId, cancellationToken);

        if (lot is null)
        {
            return Result.WithRootError(LotNotFound);
        }

        if (lot.IsFinished)
        {
            return Result.WithRootError(LotAlreadyFinished);
        }

        return lot;
    }
}
