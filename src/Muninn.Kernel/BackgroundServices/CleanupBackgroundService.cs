using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Muninn.Kernel.Common;
using Muninn.Kernel.Extensions;
using Muninn.Kernel.Models;
using ZLinq;

namespace Muninn.Kernel.BackgroundServices;

internal class CleanupBackgroundService(ILogger<BackgroundService> logger, IBackgroundManager backgroundManager, IResidentCache residentCache) : BackgroundService
{
    private readonly ILogger _logger = logger;
    private readonly IBackgroundManager _backgroundManager = backgroundManager;
    private readonly IResidentCache _residentCache = residentCache;
    private readonly TimeSpan _delayTime = TimeSpan.FromMilliseconds(200);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var expiredEntries = _backgroundManager.GetExpiredEntries();

            foreach (var entry in expiredEntries.ToArray())
            {
                var residentTask = _residentCache.RemoveAsync(entry.Key, stoppingToken);
                var enqueueTask = _backgroundManager.EnqueueDeletionAsync(new PersistentCommand(new Entry(entry.Key, [], null!, TimeSpan.Zero)), stoppingToken);
                await Task.WhenAll(residentTask, enqueueTask);
                var residentResult = residentTask.Result;

                if (!residentResult.IsSuccessful)
                {
                    _logger.LogExpiredKeyDeleteError(entry.Key, residentResult.Exception);
                    continue;
                }

                _backgroundManager.RemoveCleanupEntry(entry);
            }

            await Task.Delay(_delayTime, stoppingToken);
        }
    }
}
