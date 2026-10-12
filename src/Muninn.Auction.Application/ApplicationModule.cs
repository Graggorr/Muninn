using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Muninn.Auction.Application.Behaviors;
using Muninn.Auction.Application.Common;
using Muninn.Auction.Application.Contracts.Pipeline;
using Muninn.Auction.Application.Extensions;
using Muninn.Auction.Application.Internal;
using Muninn.Auction.Common;

namespace Muninn.Auction.Application;

public class ApplicationModule(IConfiguration configuration) : BaseModule(configuration)
{
    protected override void Load(IServiceCollection services)
    {
        base.Load(services);
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IMediator, Mediator>();
        services.AddOrleans(builder => builder.RegisterOrleans(_configuration));
        RegisterPipelineBehaviors(services);
        RegisterImplementationsOf(services, typeof(IValidator<>));
        RegisterImplementationsOf(services, typeof(IRequestHandler<,>));
        RegisterImplementationsOf(services, typeof(IGrainRequestHandler<,,>));
        RegisterImplementationsOf(services, typeof(IEventHandler<>));
        RegisterImplementationsOf(services, typeof(IGrainEventHandler<,>));
    }

    /// <summary>
    /// Behaviors run in registration order: the first one registered is the outermost.
    /// Open-generic behaviors apply to every request; request-specific (closed) behaviors
    /// found in the assembly are registered after them, so they run closest to the handler.
    /// </summary>
    private void RegisterPipelineBehaviors(IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TimeTrackingBehavior<,>));
        RegisterImplementationsOf(services, typeof(IPipelineBehavior<,>));
        RegisterImplementationsOf(services, typeof(IPipelineBehavior<>));
    }

    /// <summary>
    /// Registers every concrete, non-generic class in this assembly that implements a closed
    /// form of <paramref name="openGenericInterface"/>, once per closed interface it implements.
    /// </summary>
    private void RegisterImplementationsOf(IServiceCollection services, Type openGenericInterface)
    {
        var descriptors = ModuleAssembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false })
            .SelectMany(type => type.GetInterfaces()
                .Where(@interface => @interface.IsGenericType && @interface.GetGenericTypeDefinition() == openGenericInterface)
                .Select(@interface => ServiceDescriptor.Transient(@interface, type)));

        services.TryAddEnumerable(descriptors);
    }
}
