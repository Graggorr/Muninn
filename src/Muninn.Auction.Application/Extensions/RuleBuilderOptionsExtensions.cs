using FluentValidation;

namespace Muninn.Auction.Application.Extensions;

public static class RuleBuilderOptionsExtensions
{
    public static IRuleBuilderOptions<T, TCommand> WithErrorCode<T, TCommand>(this IRuleBuilderOptions<T, TCommand> ruleBuilder,
        ErrorCode errorCode) => ruleBuilder.WithErrorCode(errorCode.ToString());
}
