using Muninn.Kernel.Common;
using System.Collections.Concurrent;

namespace Muninn.Kernel.Persistent;

internal class PersistentQueue : IPersistentQueue
{
    private readonly ConcurrentQueue<PersistentCommand> _insertQueue = [];
    private readonly ConcurrentQueue<PersistentCommand> _deleteQueue = [];

    public Task EnqueueInsertionAsync(PersistentCommand persistentCommand)
    {
        _insertQueue.Enqueue(persistentCommand);

        return Task.CompletedTask;
    }

    public bool TryDequeueInsertion(out PersistentCommand persistentCommand)
    {
        return _insertQueue.TryDequeue(out persistentCommand!);
    }

    public Task EnqueueDeletionAsync(PersistentCommand persistentCommand)
    {
        _deleteQueue.Enqueue(persistentCommand);

        return Task.CompletedTask;
    }

    public bool TryDequeueDeletion(out PersistentCommand persistentCommand)
    {
        return _deleteQueue.TryDequeue(out persistentCommand!);
    }
}
