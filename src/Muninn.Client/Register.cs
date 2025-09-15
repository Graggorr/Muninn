using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Muninn;

public static class Register
{
    public static IServiceCollection AddMuninn(this IServiceCollection services)
    {
        services.AddOptions<MuninnConfiguration>();
        services.AddSingleton<IMuninnClient, MuninnClient>();
        var serviceProvider = services.BuildServiceProvider();
        services.AddHttpClient<MuninnClient>(nameof(MuninnClient), httpClient =>
        {
            var muninnConfiguration = serviceProvider.GetRequiredService<IOptions<MuninnConfiguration>>().Value;
            httpClient.BaseAddress = new Uri(muninnConfiguration.HostName);
            httpClient.DefaultRequestHeaders.Add("x-api-key", muninnConfiguration.ApiKey);
        });

        return services;
    }
}
