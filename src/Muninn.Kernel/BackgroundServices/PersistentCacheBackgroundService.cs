using Microsoft.Extensions.Hosting;
using Muninn.Kernel.Common;
using Muninn.Kernel.Models;

namespace Muninn.Kernel.BackgroundServices;

internal class PersistentCacheBackgroundService(IPersistentCache persistentCache, IBackgroundManager persistentQueue) : BackgroundService
{
    private readonly IPersistentCache _persistentCache = persistentCache;
    private readonly IBackgroundManager _persistentQueue = persistentQueue;
    private readonly TimeSpan _delayTime = TimeSpan.FromMilliseconds(200);
    private const int MaxTryCount = 10;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (stoppingToken.IsCancellationRequested)
        {
            await Task.WhenAll(ExecuteInsertAsync(stoppingToken), ExecuteDeleteAsync(stoppingToken));
            await Task.Delay(_delayTime, stoppingToken);
        }
    }

    private async Task ExecuteInsertAsync(CancellationToken cancellationToken)
    {
        MuninnResult? result = null;

        if (_persistentQueue.TryDequeueInsertion(out var command))
        {
            result = await _persistentCache.InsertAsync(command!.Entry, cancellationToken);
        }

        await ProceedResultAsync(result, command, true, cancellationToken);
    }

    private async Task ExecuteDeleteAsync(CancellationToken cancellationToken)
    {
        MuninnResult? result = null;

        if (_persistentQueue.TryDequeueDeletion(out var command))
        {
            result = await _persistentCache.RemoveAsync(command!.Entry.Key, cancellationToken);
        }

        await ProceedResultAsync(result, command, false, cancellationToken);
    }

    private async Task ProceedResultAsync(MuninnResult? result, PersistentCommand? command, bool isInsert, CancellationToken cancellationToken)
    {
        if (result is null || command is null)
        {
            return;
        }
        
        if (result is { IsSuccessful: false, Exception: not null })
        {
            if (command.TryCount <= MaxTryCount)
            {
                command = command.IncreaseTryCount();

                if (isInsert)
                {
                    await _persistentQueue.EnqueueInsertionAsync(command, cancellationToken);

                    return;
                }

                await _persistentQueue.EnqueueDeletionAsync(command, cancellationToken);
            }
        }
    }
}
