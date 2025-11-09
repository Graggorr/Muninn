using Muninn.Kernel.Common;
using Muninn.Kernel.Models;

namespace Muninn.Kernel.Handlers;

public class SortedResidentCacheHandler(ISortedResidentCache cache) : IOptionalCacheHandler
{
    private readonly ISortedResidentCache _cache = cache;
    
    public Task AddAsync(Entry entry, CancellationToken cancellationToken = default) => _cache.AddAsync(entry, cancellationToken);

    public Task InsertAsync(Entry entry, CancellationToken cancellationToken = default) => _cache.InsertAsync(entry, cancellationToken);

    public Task UpdateAsync(Entry entry, CancellationToken cancellationToken = default) => _cache.UpdateAsync(entry, cancellationToken);

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default) => _cache.RemoveAsync(key, cancellationToken);

    public Task<MuninnResult> GetAsync(string key, CancellationToken cancellationToken = default) => _cache.GetAsync(key, cancellationToken);
    
    public Task<MuninnResult> ClearAsync(CancellationToken cancellationToken = default) => _cache.ClearAsync(cancellationToken);
}
