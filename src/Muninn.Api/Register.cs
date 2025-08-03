namespace Muninn.Api;

public static class Register
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var muninConfiguration = new MuninConfiguration();
        configuration.Bind(nameof(MuninConfiguration), muninConfiguration);

        return services;
    }
}
