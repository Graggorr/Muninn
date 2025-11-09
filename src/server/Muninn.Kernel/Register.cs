using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Muninn.Kernel.BackgroundServices;
using Muninn.Kernel.Common;
using Muninn.Kernel.Handlers;
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
        services.AddSingleton<IFilterService, FilterService>();
        services.AddHostedService<ResidentCacheSizeBackgroundService>();

        return services;
    }

    public static IServiceCollection AddOptionalCache(this IServiceCollection services, string args)
    {
        if (args.Contains("--sort"))
        {
            services.AddSingleton<ISortedResidentCache, SortedResidentCache>();
            services.TryAddEnumerable(new ServiceDescriptor(typeof(IBaseCache), typeof(SortedResidentCache),
                ServiceLifetime.Singleton));
            services.TryAddEnumerable(new ServiceDescriptor(typeof(IOptionalCacheHandler),
                typeof(SortedResidentCacheHandler), ServiceLifetime.Singleton));
        }

        if (args.Contains("--persistent"))
        {
            services.AddSingleton<IPersistentCache, PersistentCache>();
            services.TryAddEnumerable(new ServiceDescriptor(typeof(IBaseCache), typeof(PersistentCache),
                ServiceLifetime.Singleton));
            services.TryAddEnumerable(new ServiceDescriptor(typeof(IOptionalCacheHandler),
                typeof(PersistentCache), ServiceLifetime.Singleton));
        }

        return services;
    }

    public static async Task<IApplicationBuilder> UseMuninnKernelAsync(this IApplicationBuilder app)
    {
        var persistentCache = app.ApplicationServices.GetService<IPersistentCache>();

        if (persistentCache is null)
        {
            var logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger<IApplicationBuilder>();
            logger.LogDebug("There is no persistence registered.");
            
            return app;
        }

        var residentCache = app.ApplicationServices.GetRequiredService<IResidentCache>();
        persistentCache.Initialize();
        var entries = await persistentCache.GetAllAsync();
        await residentCache.InitializeAsync(entries.ToArray());

        return app;
    }
}
