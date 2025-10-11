using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using static Muninn.Grpc.MuninnService;

namespace Muninn.Grpc;

public static class Register
{
    /// <summary>
    /// Register Muninn client into the dependency injection
    /// </summary>
    /// <param name="services">Collection of the services to build <see cref="IServiceProvider"/></param>
    /// <returns><paramref name="services"/></returns>
    public static IServiceCollection AddMuninnGrpc(this IServiceCollection services)
    {
        services.AddOptions<MuninnConfiguration>();
        services.AddGrpcClient<MuninnServiceClient>((serviceProvider, options) =>
        {
            var configuration = serviceProvider.GetRequiredService<IOptions<MuninnConfiguration>>().Value;
            options.Address = new Uri(configuration.HostName);
        });
        services.AddSingleton<IMuninnClientGrpc, MuninnClientGrpc>();
        
        return services;
    }
}
