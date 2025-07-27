using Muninn.Kernel.Models;

namespace Muninn.Kernel.Common;

public interface ICacheManager
{
    public Task<MuninResult> AddAsync(Entry entry, CancellationToken cancellationToken);

    public Task<MuninResult> RemoveAsync(string key, CancellationToken cancellationToken);

    public Task<MuninResult> GetAsync(string key, CancellationToken cancellationToken);

    public Task<IEnumerable<Entry>> GetAllAsync(CancellationToken cancellationToken);

    public Task<IEnumerable<Entry>> GetEntriesByKeyFiltersAsync(IEnumerable<IEnumerable<KeyFilter>> chunks, CancellationToken cancellationToken);

    public Task<IEnumerable<Entry>> GetEntriesByValueFiltersAsync(IEnumerable<IEnumerable<ValueFilter>> chunks, CancellationToken cancellationToken);

    public Task<MuninResult> UpdateAsync(Entry entry, CancellationToken cancellationToken);

    public Task<MuninResult> InsertAsync(Entry entry, CancellationToken cancellationToken);

    public Task InitializeAsync();

    public void Clear(CancellationToken cancellationToken);
}
