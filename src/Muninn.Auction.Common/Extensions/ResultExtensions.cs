using FluentResults;

namespace Muninn.Auction.Common.Extensions;

public static class ResultExtensions
{
    extension(ResultBase result)
    {
        public static Result WithRootError(Enum errorCode) => new Result().WithError(errorCode.ToString());
        
        public string ToStringError() => string.Join(',', result.Errors.SelectMany(error => error.Message));
    }
}
