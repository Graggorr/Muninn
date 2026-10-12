using FluentValidation;
using Muninn.Auction.Application.Contracts.Commands.Lots;

namespace Muninn.Auction.Application.Lots;

internal sealed class PlaceBidRequestValidator : AbstractValidator<PlaceBidRequest>
{
    public PlaceBidRequestValidator()
    {
        RuleFor(request => request.LotId)
            .NotEmpty();
        RuleFor(request => request.UserId)
            .NotEmpty();
        RuleFor(request => request.Amount)
            .GreaterThan(ulong.MinValue);
    }
}
