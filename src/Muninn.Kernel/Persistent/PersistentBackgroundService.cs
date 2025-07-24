using Microsoft.Extensions.Hosting;
using Muninn.Kernel.Common;

namespace Muninn.Kernel.Persistent;

internal class PersistentBackgroundService(IPersistentCache persistentCache, IPersistentQueue persistentQueue) : BackgroundService
{
    private readonly IPersistentCache _persistentCache = persistentCache;
    private readonly IPersistentQueue _persistentQueue = persistentQueue;
    private const int MAX_TRY_COUNT = 10;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (stoppingToken.IsCancellationRequested)
        {
            await Task.WhenAll(ExecuteCoreAsync(true, stoppingToken), ExecuteCoreAsync(false, stoppingToken));
        }
    }

    private async Task ExecuteCoreAsync(bool isInsert, CancellationToken cancellationToken)
    {
        MuninResult? result = null;
        PersistentCommand command;

        if (isInsert)
        {
            if (_persistentQueue.TryDequeueInsertion(out command))
            {
                result = await _persistentCache.InsertAsync(command.Entry, cancellationToken);
            }
        }
        else
        {
            if (_persistentQueue.TryDequeueDeletion(out command))
            {
                result = await _persistentCache.RemoveAsync(command.Entry.Key, cancellationToken);
            }
        }

        if (result is { IsSuccessful: false, Exception: not null })
        {
            if (command.TryCount <= MAX_TRY_COUNT)
            {
                command = command.IncreaseTryCount();

                if (isInsert)
                {
                    await _persistentQueue.EnqueueInsertionAsync(command);

                    return;
                }

                await _persistentQueue.EnqueueDeletionAsync(command);
            }
        }
    }
}
