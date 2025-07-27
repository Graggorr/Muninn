using Muninn.Kernel.Models;

namespace Muninn.Kernel.Common;

public interface ISortedResidentCache
{
    public bool IsSorting { get; }

    public void Sort(Entry[] entries);

    public MuninResult GetByKey(string key);
}
