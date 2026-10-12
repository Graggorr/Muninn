using FluentResults;
using FluentValidation;
using Muninn.Auction.Application.Contracts.Pipeline;
using static FluentResults.Result;

namespace Muninn.Auction.Application.Behaviors;

internal sealed class ValidationBehavior<TRequest, TResult>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResult> where TRequest : IRequest<TResult>
{
    private readonly IValidator<TRequest>[] _validators = [.. validators];

    public async Task<Result<TResult>> HandleAsync(TRequest request, RequestHandleDelegate<TResult> next,
        CancellationToken cancellationToken = default)
    {
        var context = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(_validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));
        var failures = results.SelectMany(result => result.Errors).ToList();

        return failures.Count > 0
            ? Fail<TResult>(failures.Select(failure => failure.ErrorCode))
            : await next(cancellationToken);
    }
}
