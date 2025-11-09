using Microsoft.Extensions.DependencyInjection;

namespace Muninn;

public static class Register
{
    /// <summary>
    /// Register Muninn client into the dependency injection
    /// </summary>
    /// <param name="services">Collection of the services to build <see cref="IServiceProvider"/></param>
    /// <returns><paramref name="services"/></returns>
    public static IServiceCollection AddMuninn(this IServiceCollection services)
    {
        services.AddOptions<MuninnConfiguration>();
        services.AddSingleton<IMuninnClient, MuninnClient>();
        services.AddHttpClient<MuninnClient>(nameof(MuninnClient));

        return services;
    }
}
