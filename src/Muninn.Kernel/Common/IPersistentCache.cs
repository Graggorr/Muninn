using Muninn.Kernel.Models;

namespace Muninn.Kernel.Common;

internal interface IPersistentCache
{
    public Task<MuninResult> RemoveAsync(string key, CancellationToken cancellationToken);

    public Task<MuninResult> GetAsync(string key, CancellationToken cancellationToken);

    public Task<IEnumerable<Entry>> GetAllAsync(CancellationToken cancellationToken);

    public Task<IEnumerable<Entry>> GetEntriesByKeyFiltersAsync(IEnumerable<IEnumerable<KeyFilter>> chunks, CancellationToken cancellationToken);

    public Task<IEnumerable<Entry>> GetEntriesByValueFiltersAsync(IEnumerable<IEnumerable<ValueFilter>> chunks, CancellationToken cancellationToken);

    public Task<MuninResult> InsertAsync(Entry entry, CancellationToken cancellationToken);

    public Task ClearAsync(CancellationToken cancellationToken);

    public void Initialize();
}
