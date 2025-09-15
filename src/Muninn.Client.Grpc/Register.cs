using Microsoft.Extensions.DependencyInjection;

namespace Muninn.Grpc;

public static class Register
{
    public static IServiceCollection AddMuninnGrpc(this IServiceCollection services)
    {
        services.AddSingleton<IMuninnClient, MuninnClientGrpc>();

        return services;
    }
}
