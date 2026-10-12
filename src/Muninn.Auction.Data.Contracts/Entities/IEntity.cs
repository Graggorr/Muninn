using Muninn.Auction.Data.Contracts.Common;

namespace Muninn.Auction.Data.Contracts.Entities;

public interface IEntity : ICreated
{
    Guid Id { get; init; }
}
