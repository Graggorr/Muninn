using Muninn.Kernel.Models;

namespace Muninn.Kernel.Common;

public interface ISortedResidentCache : IResidentCache, IBaseCache
{
    public Task<MuninnResult> SortAsync(CancellationToken cancellationToken = default);
}
