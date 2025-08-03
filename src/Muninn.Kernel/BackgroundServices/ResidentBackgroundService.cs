using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Muninn.Kernel.Common;
using Muninn.Kernel.Extensions;

namespace Muninn.Kernel.BackgroundServices;

internal class ResidentBackgroundService(ILogger<BackgroundService> logger, ISortedResidentCache sortedResidentCache, IResidentCache residentCache) : BackgroundService
{
    private readonly ILogger _logger = logger;
    private readonly ISortedResidentCache _sortedResidentCache = sortedResidentCache;
    private readonly IResidentCache _residentCache = residentCache;
    private readonly TimeSpan _baseSleepTime = TimeSpan.FromMilliseconds(200);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var entries = _residentCache.GetAll(stoppingToken);
            var timestamp = Stopwatch.GetTimestamp();
            await _sortedResidentCache.SortAsync(entries.ToArray());
            var elapsedTime = Stopwatch.GetElapsedTime(timestamp);

            if (elapsedTime > TimeSpan.FromMinutes(1))
            {
                _logger.LogSlowSorting(elapsedTime);
            }

            await Task.Delay(_baseSleepTime + elapsedTime, stoppingToken);
        }
    }
}
