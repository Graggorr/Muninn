namespace Muninn.Auction.Application.Contracts.Pipeline;

public interface IRequest;

public interface IRequest<out TResult> : IRequest;
