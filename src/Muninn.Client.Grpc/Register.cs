using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using static Muninn.Grpc.MuninnService;

namespace Muninn.Grpc;

public static class Register
{
    public static IServiceCollection AddMuninnGrpc(this IServiceCollection services)
    {
        services.AddOptions<MuninnConfiguration>();
        services.AddGrpcClient<MuninnServiceClient>((serviceProvider, options) =>
        {
            var configuration = serviceProvider.GetRequiredService<IOptions<MuninnConfiguration>>().Value;
            options.Address = new Uri(configuration.HostName);
            options.CallOptionsActions.Add(callOptionsContext =>
            {
                callOptionsContext.CallOptions.WithHeaders(new()
                {
                    { "x-api-key", configuration.ApiKey }
                });
            });
        });
        services.AddSingleton<IMuninnClientGrpc, MuninnClientGrpc>();
        
        return services;
    }
}
