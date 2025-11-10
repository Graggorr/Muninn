using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Muninn.Kernel.Common;
using Muninn.Kernel.Extensions;

namespace Muninn.Kernel.BackgroundServices;

public class ResidentCacheSizeBackgroundService : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IResidentCache _residentCache;
    private readonly TimeSpan _timeSpan = TimeSpan.FromSeconds(10);
    private readonly List<Func<CancellationToken, Task>> _increaseTasks;
    private readonly List<Func<CancellationToken, Task>> _decreaseTasks;
    
    private const int MinimumDifferenceToIncrease = 100;
    private const int MinimumDifferenceToDecrease = 1000;

    public ResidentCacheSizeBackgroundService(ILogger<BackgroundService> logger, IResidentCache residentCache, IEnumerable<IBaseCache> baseCaches)
    {
        var sortedResidentCache = baseCaches.FirstOrDefault(cache => cache is ISortedResidentCache) as ISortedResidentCache;
        _logger = logger;
        _residentCache = residentCache;
        _increaseTasks = new List<Func<CancellationToken, Task>>(2)
        {
            _residentCache.IncreaseArraySizeAsync,
        };
        _decreaseTasks = new List<Func<CancellationToken, Task>>(2)
        {
            _residentCache.DecreaseArraySizeAsync,
        };

        if (sortedResidentCache is not null)
        {
            _increaseTasks.Add(sortedResidentCache.IncreaseArraySizeAsync);
            _decreaseTasks.Add(sortedResidentCache.DecreaseArraySizeAsync);
        }
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var difference = _residentCache.Length - _residentCache.Count;

            switch (difference)
            {
                case <= MinimumDifferenceToIncrease:
                    await Task.WhenAll(_increaseTasks.Select(task => task(stoppingToken)));
                    break;
                case >= MinimumDifferenceToDecrease:
                    await Task.WhenAll(_decreaseTasks.Select(task => task(stoppingToken)));
                    break;
            }
            
            await Task.Delay(_timeSpan, stoppingToken);
        }
        
        _logger.LogBackgroundServiceShutdown(nameof(ResidentCacheSizeBackgroundService));
    }
}
