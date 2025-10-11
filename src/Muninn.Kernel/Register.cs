using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Muninn.Kernel.BackgroundServices;
using Muninn.Kernel.Common;
using Muninn.Kernel.Persistent;
using Muninn.Kernel.Resident;
using Muninn.Kernel.Shared;

namespace Muninn.Kernel;

public static class Register
{
    public static IServiceCollection AddMuninKernel(this IServiceCollection services)
    {
        services.AddSingleton<ICacheManager, CacheManager>();
        services.AddSingleton<IResidentCache, ResidentCache>();
        services.AddSingleton<IPersistentCache, PersistentCache>();
        services.AddSingleton<ISortedResidentCache, SortedResidentCache>();
        services.AddSingleton<IFilterManager, FilterManager>();
        services.AddSingleton<IBackgroundManager, BackgroundManager>();
        services.AddHostedService<CleanupBackgroundService>();
        services.AddHostedService<PersistentCacheBackgroundService>();
        services.AddHostedService<ResidentCacheBackgroundService>();
        services.AddHostedService<SortedResidentCacheBackgroundService>();

        return services;
    }

    public static Task InitializeMuninAsync(this IEndpointRouteBuilder app)
    {
        var cacheManager = app.ServiceProvider.GetRequiredService<ICacheManager>();

        return cacheManager.InitializeAsync();
    }
}
