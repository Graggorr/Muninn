using FluentValidation;
using Muninn.Auction.Application.Contracts.Commands.Users;
using Muninn.Auction.Application.Extensions;
using Muninn.Auction.Data.Contracts;
using static Muninn.Auction.Application.Common.ValidationRegex;
using static Muninn.Auction.Application.ErrorCode;

namespace Muninn.Auction.Application.Users;

internal sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator(IUserRepository userRepository)
    {
        RuleFor(user => user.Username)
            .NotEmpty()
            .Matches(Username)
            .WithErrorCode(UsernameNotValid);
        RuleFor(user => user.Username)
            .MustAsync(userRepository.IsUsernameUniqueAsync)
            .WithErrorCode(UsernameNotUnique);
        RuleFor(user => user.Email)
            .NotEmpty()
            .EmailAddress()
            .WithErrorCode(EmailNotValid);
        RuleFor(user => user.Email)
            .MustAsync(userRepository.IsEmailUniqueAsync)
            .WithErrorCode(EmailNotUnique);
        RuleFor(user => user.Password)
            .NotEmpty()
            .Matches(Password)
            .WithErrorCode(PasswordNotValid);
        RuleFor(user => user.MobileNumber)
            .NotEmpty()
            .Matches(MobileNumber)
            .WithErrorCode(MobileNumberNotValid);
        RuleFor(user => user.MobileNumber)
            .MustAsync(userRepository.IsMobileNumberUniqueAsync)
            .WithErrorCode(MobileNumberNotUnique);
    }
}
