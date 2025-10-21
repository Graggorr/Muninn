using Muninn.Kernel.Models;

namespace Muninn.Kernel.Common;

public interface ISortedResidentCache : IResidentCache
{
    public Task<MuninnResult> GetAsync(string key, CancellationToken cancellationToken = default);
    
    public Task<MuninnResult> SortAsync(CancellationToken cancellationToken = default);
}
