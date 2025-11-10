using Muninn.Kernel.Models;

namespace Muninn.Kernel.Common;

public interface IOptionalCacheHandler
{
    public Task AddAsync(Entry entry, CancellationToken cancellationToken = default);
    
    public Task InsertAsync(Entry entry, CancellationToken cancellationToken = default);
    
    public Task<MuninnResult> GetAsync(string key, CancellationToken cancellationToken = default);
    
    public Task<IEnumerable<Entry>> GetAllAsync(bool isTracking, CancellationToken cancellationToken = default);
    
    public Task UpdateAsync(Entry entry, CancellationToken cancellationToken = default);
    
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    
    public Task<MuninnResult> ClearAsync(CancellationToken cancellationToken = default);
}
