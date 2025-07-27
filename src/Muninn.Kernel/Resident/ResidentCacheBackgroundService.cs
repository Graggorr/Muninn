using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Muninn.Kernel.Common;

namespace Muninn.Kernel.Resident;

internal class ResidentCacheBackgroundService(ISortedResidentCache sortedResidentCache, IResidentCache residentCache) : BackgroundService
{
    private readonly ISortedResidentCache _sortedResidentCache = sortedResidentCache;
    private readonly IResidentCache _residentCache = residentCache;
    private readonly TimeSpan _baseSleepTime = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var entries = _residentCache.GetAll(stoppingToken);
            var timestamp = Stopwatch.GetTimestamp();
            _sortedResidentCache.Sort(entries.ToArray());
            var elapsedTime = Stopwatch.GetElapsedTime(timestamp);
            await Task.Delay(_baseSleepTime + elapsedTime, stoppingToken);
        }
    }
}
