using Muninn.Kernel.Models;

namespace Muninn.Kernel.Common;

public interface IOrderedResidentCache
{
    public bool IsOrdering { get; }

    public void UpdateCache(Entry[] entries);
}
