using Ardalis.Specification;
using Muninn.Auction.Data.Contracts.Entities;

namespace Muninn.Auction.Data.Specifications;

public sealed class UserSpecification : Specification<User>
{
    public UserSpecification(Guid userId)
    {
        Query.Where(user => user.Id == userId)
            .Include(user => user.HistoryLots)
            .ThenInclude(lot => lot.Bids)
            .ThenInclude(bid => bid.Owner);
    }

    public UserSpecification(Guid userId, bool isWonLots)
    {
        Query.Where(user => user.Id == userId)
            .Include(user => user.HistoryLots)
            .ThenInclude(lot => lot.Bids)
            .ThenInclude(bid => bid.Owner);
    }

}
