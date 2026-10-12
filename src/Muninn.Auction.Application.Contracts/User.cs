namespace Muninn.Auction.Application.Contracts;

[GenerateSerializer]
public sealed record User
{
    [Id(0)]
    public required Guid Id { get; init; }

    [Id(1)]
    public required string Username { get; init; }

    [Id(2)]
    public required string Email { get; init; }

    [Id(3)]
    public required string MobileNumber { get; init; }

    [Id(4)]
    public bool IsMobileNumberConfirmed { get; init; }

    [Id(5)]
    public bool IsEmailConfirmed { get; init; }

    [Id(6)]
    public ulong Amount { get; init; }

    [Id(7)]
    public List<Lot> Lots { get; init; } = [];
}
