using Ardalis.Specification;
using Muninn.Auction.Data.Contracts.Entities;

namespace Muninn.Auction.Data.Specifications;

internal sealed class LotSpecification : Specification<Lot>
{
    public LotSpecification(Guid lotId)
    {
        Query.Where(lot => lot.Id == lotId && !lot.IsDeleted)
            .Include(lot => lot.Owner)
            .Include(lot => lot.WinnerBid)
            .Include(lot => lot.Bids)
            .ThenInclude(bid => bid.Owner);
    }
}
