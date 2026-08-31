using Muninn.Auction.Data.Common;

namespace Muninn.Auction.Data.Entities;

public interface IEntity : ICreated
{
    Guid Id { get; init; }
}
