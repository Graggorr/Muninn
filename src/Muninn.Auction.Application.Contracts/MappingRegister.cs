using Mapster;
using Muninn.Auction.Application.Contracts.Commands;
using Muninn.Auction.Application.Contracts.Commands.Lots;
using Muninn.Auction.Application.Contracts.States;
using ApplicationBid = Muninn.Auction.Application.Contracts.Bid;
using DataBid = Muninn.Auction.Data.Contracts.Entities.Bid;
using ApplicationLot = Muninn.Auction.Application.Contracts.Lot;
using DataLot = Muninn.Auction.Data.Contracts.Entities.Lot;

namespace Muninn.Auction.Application.Contracts;

public sealed class MappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<PlaceBidRequest, DataBid>()
            .Map(destination => destination.Id, _ => Guid.CreateVersion7())
            .Map(destination => destination.OwnerId, source => source.UserId)
            .Ignore(destination => destination.Owner)
            .Ignore(destination => destination.Lot);

        config.NewConfig<DataBid, ApplicationBid>()
            .Ignore(destination => destination.IsBuyout);
        
        config.NewConfig<OpenLotRequest, DataLot>()
            .Ignore(destination => destination.Id)
            .Ignore(destination => destination.Owner)
            .Ignore(destination => destination.WinnerBid)
            .Ignore(destination => destination.Bids)
            .Ignore(destination => destination.Participants);

        config.NewConfig<OpenLotRequest, LotState>()
            .Map(destination => destination.Exists, _ => true)
            .Map(destination => destination.CurrentPrice, source => source.StartPrice);

        config.NewConfig<DataLot, LotState>()
            .Map(destination => destination.Exists, _ => true)
            .Map(destination => destination.CurrentPrice,
                source => source.MaxPrice > 0 ? source.MaxPrice : source.StartPrice)
            .Map(destination => destination.BidCount, source => source.Bids.Count);

        config.NewConfig<LotState, ApplicationLot>()
            .Ignore(destination => destination.Id);
    }
}
