using Muninn.Server.Shared;

namespace Muninn.Api;

public static class Register
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddOptions<MuninnConfiguration>();
        
        return services;
    }
}
