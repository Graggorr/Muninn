using Muninn.Kernel.Common;
using Muninn.Kernel.Models;
using System.Collections.Concurrent;

namespace Muninn.Kernel.Shared;

internal class BackgroundManager : IBackgroundManager
{
    private readonly ConcurrentQueue<PersistentCommand> _insertQueue = [];
    private readonly ConcurrentQueue<PersistentCommand> _deleteQueue = [];
    private readonly ConcurrentHashSet<CleanupEntry> _cleanupEntries = [];

    public Task EnqueueInsertionAsync(PersistentCommand persistentCommand, CancellationToken cancellationToken) => Task.Factory.StartNew(() =>
    {
        _insertQueue.Enqueue(persistentCommand);
        _cleanupEntries.Add(new CleanupEntry(persistentCommand.Entry.Key, DateTime.UtcNow + persistentCommand.Entry.LifeTime));
    }, cancellationToken);

    public bool TryDequeueInsertion(out PersistentCommand? persistentCommand) => _insertQueue.TryDequeue(out persistentCommand);

    public Task EnqueueDeletionAsync(PersistentCommand persistentCommand, CancellationToken cancellationToken) => Task.Factory.StartNew(() =>
    {
        _deleteQueue.Enqueue(persistentCommand);
    }, cancellationToken);

    public bool TryDequeueDeletion(out PersistentCommand? persistentCommand) => _deleteQueue.TryDequeue(out persistentCommand);

    public void RemoveCleanupEntry(CleanupEntry cleanupEntry) => _cleanupEntries.Remove(cleanupEntry);

    public IEnumerable<CleanupEntry> GetExpiredEntries() => _cleanupEntries.Where(entry => entry.ExpirationTime <= DateTime.UtcNow);
}
