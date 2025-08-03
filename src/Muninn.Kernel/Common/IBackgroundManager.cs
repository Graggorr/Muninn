using Muninn.Kernel.Models;
using ZLinq;
using ZLinq.Linq;

namespace Muninn.Kernel.Common;

internal interface IBackgroundManager
{
    public Task EnqueueInsertionAsync(PersistentCommand persistentCommand, CancellationToken cancellationToken);

    public bool TryDequeueInsertion(out PersistentCommand? persistentCommand);

    public Task EnqueueDeletionAsync(PersistentCommand persistentCommand, CancellationToken cancellationToken);

    public bool TryDequeueDeletion(out PersistentCommand? persistentCommand);

    public void RemoveCleanupEntry(CleanupEntry cleanupEntry);

    public ValueEnumerable<Where<FromEnumerable<CleanupEntry>, CleanupEntry>, CleanupEntry> GetExpiredEntries();
}
