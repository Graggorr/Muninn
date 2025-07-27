using Muninn.Kernel.Models;

namespace Muninn.Kernel.Persistent;

internal record PersistentCommand(Entry Entry, int TryCount = 0)
{
    public PersistentCommand IncreaseTryCount() => this with { TryCount = TryCount + 1 };
}
