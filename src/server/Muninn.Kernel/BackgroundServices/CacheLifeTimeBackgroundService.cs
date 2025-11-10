using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Muninn.Kernel.Common;
using Muninn.Kernel.Extensions;

namespace Muninn.Kernel.BackgroundServices;

public class CacheLifeTimeBackgroundService(ILogger<BackgroundService> logger, ICacheManager cacheManager) : BackgroundService
{
    private readonly ILogger _logger = logger;
    private readonly ICacheManager _cacheManager = cacheManager;
    private readonly TimeSpan _delayTime = TimeSpan.FromSeconds(10);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var result = await _cacheManager.GetAllAsync(true, stoppingToken);
            var targets = result.Where(entry => !entry.LifeTime.Equals(TimeSpan.Zero) && entry.LastModificationTime.Add(entry.LifeTime) >= DateTime.UtcNow).ToList();

            if (targets.Any())
            {
                var maxThreadsCount = Math.Min(targets.Count, 100);
                var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = maxThreadsCount, CancellationToken = stoppingToken };
                await Parallel.ForEachAsync(targets, parallelOptions, async (target, cancellationToken) =>
                {
                    await _cacheManager.RemoveAsync(target.Key, cancellationToken); 
                });
            }

            await Task.Delay(_delayTime, stoppingToken);
        }
        
        _logger.LogBackgroundServiceShutdown(nameof(CacheLifeTimeBackgroundService));
    }
}