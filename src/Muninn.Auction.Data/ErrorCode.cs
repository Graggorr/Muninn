namespace Muninn.Auction.Data;

public enum ErrorCode
{
    UserNotFound,
    LotNotFound,
    LotAlreadyExists,
    LotAlreadyFinished,
    StartPriceCannotBeChanged,
    BidTooLow,
    OwnerCannotBidOnOwnLot,
    StartPriceHigherThanMaxPrice,
    StartPriceHigherThanBuyoutPrice,
    MaxPriceLower,
    BuyoutPriceLower,
    BuyoutPriceLowerLimit,
}
