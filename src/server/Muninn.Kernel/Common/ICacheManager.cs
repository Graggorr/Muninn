using Muninn.Kernel.Models;

namespace Muninn.Kernel.Common;

public interface ICacheManager
{
    public Task<MuninnResult> AddAsync(Entry entry, CancellationToken cancellationToken = default);

    public Task<MuninnResult> RemoveAsync(string key, CancellationToken cancellationToken = default);

    public Task<MuninnResult> GetAsync(string key, CancellationToken cancellationToken = default);

    public Task<IEnumerable<Entry>> GetAllAsync(bool isTracking, CancellationToken cancellationToken = default);
    
    public Task<IEnumerable<Entry>> GetEntriesByKeyFiltersAsync(IEnumerable<IEnumerable<KeyFilter>> chunks, CancellationToken cancellationToken = default);

    public Task<IEnumerable<Entry>> GetEntriesByValueFiltersAsync(IEnumerable<IEnumerable<ValueFilter>> chunks, CancellationToken cancellationToken = default);

    public Task<MuninnResult> UpdateAsync(Entry entry, CancellationToken cancellationToken = default);

    public Task<MuninnResult> InsertAsync(Entry entry, CancellationToken cancellationToken = default);

    public Task<MuninnResult> ClearAsync(CancellationToken cancellationToken = default);
}
