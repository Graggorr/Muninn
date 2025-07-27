using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Muninn.Kernel.Common;
using Muninn.Kernel.Persistent;
using Muninn.Kernel.Resident;

namespace Muninn.Kernel;

public static class Register
{
    public static IServiceCollection AddMuninKernel(this IServiceCollection services)
    {
        services.AddMuninConfiguration();
        services.AddSingleton<ICacheManager, CacheManager>();
        services.AddSingleton<IResidentCache, ResidentCache>();
        services.AddSingleton<IPersistentCache, PersistentCache>();
        services.AddSingleton<ISortedResidentCache, SortedResidentCache>();
        services.AddSingleton<IPersistentQueue, PersistentQueue>();
        services.AddSingleton<IFilterManager, FilterManager>();
        services.AddHostedService<PersistentBackgroundService>();
        services.AddHostedService<ResidentCacheBackgroundService>();

        return services;
    }

    private static IServiceCollection AddMuninConfiguration(this IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var persistentConfiguration = new PersistentConfiguration();
        var residentConfiguration = new ResidentConfiguration();
        configuration.Bind(nameof(PersistentConfiguration), persistentConfiguration);
        configuration.Bind(nameof(ResidentConfiguration), residentConfiguration);
        services.AddSingleton(persistentConfiguration);
        services.AddSingleton(residentConfiguration);

        return services;
    }

    private static Task InitializeMuninAsync(this IEndpointRouteBuilder app)
    {
        var cacheManager = app.ServiceProvider.GetRequiredService<ICacheManager>();

        return cacheManager.InitializeAsync();
    }
}
