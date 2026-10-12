namespace Muninn.Auction.Application.Contracts.States;

[GenerateSerializer]
public sealed record UserState
{
    [Id(0)]
    public required Guid Id { get; set; }

    [Id(1)]
    public required bool IsDeleted { get; set; }

    [Id(2)]
    public required string Username { get; set; }

    [Id(3)]
    public required string HashedPassword { get; set; }

    [Id(4)]
    public required string Email { get; set; }

    [Id(5)]
    public required string MobileNumber { get; set; }

    [Id(6)]
    public required bool IsMobileNumberConfirmed { get; set; }

    [Id(7)]
    public required bool IsEmailConfirmed { get; set; }

    [Id(8)]
    public required ulong Amount { get; set; }

    [Id(9)]
    public required List<Lot> Lots { get; set; } = [];
}
