using Microsoft.Extensions.Hosting;
using Muninn.Kernel.Common;

namespace Muninn.Kernel.BackgroundServices;

public class ResidentCacheBackgroundService(IResidentCache residentCache, ISortedResidentCache sortedResidentCache) : BackgroundService
{
    private readonly IResidentCache _residentCache = residentCache;
    private readonly ISortedResidentCache _sortedResidentCache = sortedResidentCache;
    private readonly TimeSpan _timeSpan = TimeSpan.FromSeconds(10);
    
    private const int MinimumDifferenceToIncrease = 100;
    private const int MinimumDifferenceToDecrease = 1000;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var difference = _residentCache.Length - _residentCache.Count;

            switch (difference)
            {
                case <= MinimumDifferenceToIncrease:
                    await _residentCache.IncreaseArraySizeAsync(stoppingToken);
                    await _sortedResidentCache.IncreaseArraySizeAsync(stoppingToken);
                    break;
                case >= MinimumDifferenceToDecrease:
                    await _residentCache.DecreaseArraySizeAsync(stoppingToken);
                    await _sortedResidentCache.DecreaseArraySizeAsync(stoppingToken);
                    break;
            }
            
            await Task.Delay(_timeSpan, stoppingToken);
        }
    }
}