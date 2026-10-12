using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using Muninn.Auction.Application.Contracts.Pipeline;

namespace Muninn.Auction.Application.Internal;

internal sealed class Mediator(IServiceScopeFactory serviceScopeFactory, IGrainFactory grainFactory) : IMediator
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly IGrainFactory _grainFactory = grainFactory;

    public async Task<Result<TResult>> SendAsync<TRequest, TResult, TGrain>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IGrainRequest<TResult, TGrain> where TGrain : IGrainWithGuidKey
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var serviceProvider = scope.ServiceProvider;
        var handler = serviceProvider.GetRequiredService<IGrainRequestHandler<TRequest, TResult, TGrain>>();

        return await ExecuteAsync<TRequest, TResult>(serviceProvider, request,
            token => handler.HandleAsync(request, _grainFactory.GetGrain<TGrain>(request.GrainId), token), cancellationToken);
    }

    public async Task<Result<TResult>> SendAsync<TRequest, TResult>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResult>
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var serviceProvider = scope.ServiceProvider;
        var handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResult>>();

        return await ExecuteAsync<TRequest, TResult>(serviceProvider, request,
            token => handler.HandleAsync(request, token), cancellationToken);
    }

    public async Task<Result> PublishAsync<TEvent, TGrain>(TEvent grainEvent, CancellationToken cancellationToken = default) where TEvent : IGrainEvent<TGrain> where TGrain : IGrainWithGuidKey
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var serviceProvider = scope.ServiceProvider;
        var handler = serviceProvider.GetRequiredService<IGrainEventHandler<TEvent, TGrain>>();

        return await ExecuteAsync(serviceProvider, grainEvent,
            token => handler.HandleAsync(grainEvent, _grainFactory.GetGrain<TGrain>(grainEvent.GrainId), token), cancellationToken);
    }

    public async Task<Result> PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var serviceProvider = scope.ServiceProvider;
        var handler = serviceProvider.GetRequiredService<IEventHandler<TEvent>>();

        return await ExecuteAsync(serviceProvider, @event,
            token => handler.HandleAsync(@event, token), cancellationToken);
    }

    /// <summary>
    /// Wraps <paramref name="handler"/> in the registered <see cref="IPipelineBehavior{TRequest,TResult}"/>s,
    /// first registered outermost, and runs the resulting pipeline.
    /// </summary>
    private static Task<Result<TResult>> ExecuteAsync<TRequest, TResult>(IServiceProvider serviceProvider, TRequest request,
        RequestHandleDelegate<TResult> handler, CancellationToken cancellationToken) where TRequest : IRequest<TResult>
    {
        var pipeline = serviceProvider
            .GetServices<IPipelineBehavior<TRequest, TResult>>()
            .Reverse()
            .Aggregate(handler, (next, behavior) => token => behavior.HandleAsync(request, next, token));

        return pipeline(cancellationToken);
    }

    private static Task<Result> ExecuteAsync<TEvent>(IServiceProvider serviceProvider, TEvent @event,
        RequestHandleDelegate handler, CancellationToken cancellationToken) where TEvent : IEvent
    {
        var pipeline = serviceProvider
            .GetServices<IPipelineBehavior<TEvent>>()
            .Reverse()
            .Aggregate(handler, (next, behavior) => token => behavior.HandleAsync(@event, next, token));

        return pipeline(cancellationToken);
    }
}
