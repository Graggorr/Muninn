using Muninn.Kernel.Persistent;

namespace Muninn.Kernel.Common;

public interface IPersistentQueue
{
    public Task EnqueueInsertionAsync(PersistentCommand persistentCommand);

    public bool TryDequeueInsertion(out PersistentCommand persistentCommand);

    public Task EnqueueDeletionAsync(PersistentCommand persistentCommand);

    public bool TryDequeueDeletion(out PersistentCommand persistentCommand);
}
