using Muninn.Kernel.Models;

namespace Muninn.Kernel.Common;

public interface ISortedResidentCache
{
    public bool IsSorting { get; }

    public Task SortAsync(Entry[] entries);

    public MuninnResult GetByKey(string key);
}
